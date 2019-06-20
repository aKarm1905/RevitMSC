﻿

using System;
using System.Collections.Generic;
using ProtoCore.DSASM;
using ProtoCore.Lang.Replication;
using ProtoCore.Properties;
using ProtoCore.Utils;
using ProtoFFI;

namespace ProtoCore.Lang
{
    public class FFIActivationRecord
    {
        public string ModuleType;
        public string ModuleName;
        public bool IsDNI;
        public string FunctionName;
        public List<ProtoCore.Type> ParameterTypes;
        public ProtoCore.Type ReturnType;
        public JILActivationRecord JILRecord;
    }

    public class FFIFunctionEndPoint : FunctionEndPoint
    {
        public static Dictionary<string, FFIHandler> FFIHandlers = new Dictionary<string,FFIHandler>();
        public FFIActivationRecord activation;
        private FFIFunctionPointer mFunctionPointer;
        private Interpreter mInterpreter;
        private ProcedureNode mFNode;

        public FFIFunctionEndPoint(FFIActivationRecord record)
        {
            activation = record;
        }
        public FFIFunctionEndPoint()
        {
            activation = new FFIActivationRecord();
        }

        private bool Validate(FFIFunctionPointer p){ return true; /*todo implement arg type checking*/}
        private bool ValidateReturn(Object p){ return true; /*TODO implement returyn type checking*/}


        public override bool DoesPredicateMatch(ProtoCore.Runtime.Context c, List<StackValue> formalParameters, List<ReplicationInstruction> replicationInstructions)
        {
            return true;
        }

        private void Init(RuntimeCore runtimeCore)
        {
            if (mInterpreter != null) return;

            mInterpreter = new Interpreter(runtimeCore, true);

            int ci = activation.JILRecord.classIndex;
            int fi = activation.JILRecord.funcIndex;

            string className = "";
            if (Constants.kInvalidIndex != ci)
            {
                var classNode = mInterpreter.runtime.exe.classTable.ClassNodes[ci];
                mFNode = classNode.ProcTable.Procedures[fi];
                className = classNode.Name;
            }

            var argTypes = new List<Type>(activation.ParameterTypes);

            if (null != mFNode && mFNode.IsAutoGeneratedThisProc)
            {
                argTypes.RemoveAt(0);
            }

            FFIHandler handler = FFIHandlers[activation.ModuleType];

            FFIFunctionPointer functionPointer = handler.GetFunctionPointer(
                activation.ModuleName, className, activation.FunctionName, argTypes, activation.ReturnType);

            mFunctionPointer = Validate(functionPointer) ? functionPointer : null;
            mFunctionPointer.IsDNI = activation.IsDNI;

            mInterpreter.runtime.executingBlock = runtimeCore.RunningBlock;
            activation.JILRecord.globs = runtimeCore.DSExecutable.runtimeSymbols[runtimeCore.RunningBlock].GetGlobalSize();
        }

        public override StackValue Execute(Runtime.Context c, List<StackValue> formalParameters, StackFrame stackFrame, RuntimeCore runtimeCore)
        {   
            StackValue svThisPtr = stackFrame.ThisPtr;

            if (mInterpreter == null)
            {
                Init(runtimeCore);
            }

            if (mFunctionPointer == null)
            {
                return StackValue.Null;
            }

                // Check if this is a 'this' pointer function overload that was generated by the compiler
            if (null != mFNode && mFNode.IsAutoGeneratedThisProc)
            {
                int thisPtrIndex = 0;
                bool isStaticCall = svThisPtr.IsPointer && Constants.kInvalidPointer == svThisPtr.Pointer;
                if (isStaticCall)
                {
                    stackFrame.ThisPtr = formalParameters[thisPtrIndex];
                }

                // Comment Jun: Execute() can handle a null this pointer. 
                // But since we don't even need to to reach there if we don't have a valid this pointer, then just return null
                if (formalParameters[thisPtrIndex].IsNull)
                {
                    runtimeCore.RuntimeStatus.LogWarning(
                        Runtime.WarningID.DereferencingNonPointer, Resources.kDeferencingNonPointer);
                    return StackValue.Null;
                }

                // These are the op types allowed. 
                Validity.Assert(formalParameters[thisPtrIndex].IsPointer ||
                                formalParameters[thisPtrIndex].IsDefaultArgument);

                svThisPtr = formalParameters[thisPtrIndex];

                formalParameters.RemoveAt(thisPtrIndex);
            }
            
            formalParameters.Add(svThisPtr);

            Object ret = mFunctionPointer.Execute(c, mInterpreter, formalParameters);
            StackValue op;
            if (ret == null)
            {
                op = StackValue.Null;
            }
            else if (ret is StackValue)
            {
                op = (StackValue) ret;
            }
            else if (ret is Int64 || ret is int)
            {
                op = StackValue.BuildInt((Int64) ret);
            }
            else if (ret is double)
            {
                op = StackValue.BuildDouble((double) ret);
            }
            else
            {
                throw new ArgumentException(string.Format("FFI: incorrect return type {0} from external function {1}:{2}", 
                    activation.ReturnType.Name, activation.ModuleName, activation.FunctionName));
            }
            return op;
        }
    }
}