﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pyRevitLabs.Common {
    // ERROR CODES ===================================================================================================
    // error codes are to be used for non-critical, non-breaking errors

    // available error codes
    public enum ErrorCodes {
        NoErrors,
        DefaultedToAllUsersConfigFile,
        MoreThanOneItemMatched,
    }

    // error handling singleton
    // >>> Errors.LatestError = ErrorCodes.OccuredErrorCode;
    public sealed class Errors {
        private static Errors instance = null;
        private static readonly object padlock = new object();

        Errors() {
        }

        public static Errors Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new Errors();
                    }
                    return instance;
                }
            }
        }

        public static ErrorCodes LatestError { get; set; } = ErrorCodes.NoErrors;
    }

    // EXCEPTIONS ====================================================================================================
    // exceptions to be used for all breaking, critical errors

    // base exception
    public class pyRevitException : Exception {
        public pyRevitException() { }

        public pyRevitException(string message) : base(message) { }

        public pyRevitException(string message, Exception innerException) : base(message, innerException) { }
    }

    // resource exceptions
    public class pyRevitResourceMissingException : pyRevitException {
        public pyRevitResourceMissingException() { }

        public pyRevitResourceMissingException(string resoucePath) { Path = resoucePath; }

        public string Path { get; set; }

        public override string Message {
            get {
                return String.Format("\"{0}\" does not exist.", Path);
            }
        }
    }

    public class pyRevitNoInternetConnectionException : pyRevitException {
        public pyRevitNoInternetConnectionException() { }

        public override string Message {
            get {
                return "No internet connection detected.";
            }
        }

    }

}
