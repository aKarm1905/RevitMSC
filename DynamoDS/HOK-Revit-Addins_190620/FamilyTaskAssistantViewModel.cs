﻿#region References
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Tools.Communicator.Messaging;
using RelayCommand = GalaSoft.MvvmLight.Command.RelayCommand;
#endregion

namespace HOK.MissionControl.Tools.Communicator.Tasks.FamilyTaskAssistant
{
    public class FamilyTaskAssistantViewModel : ViewModelBase
    {
        public string Title { get; set; }
        public Window Win { get; set; }
        public FamilyTaskAssistantModel Model { get; set; }
        public RelayCommand<Window> Complete { get; set; }
        public RelayCommand EditFamily { get; set; }
        public RelayCommand<Window> Close { get; set; }
        public RelayCommand<Window> WindowLoaded { get; set; }
        public RelayCommand<Window> WindowClosed { get; set; }
        private readonly object _lock = new object();

        public FamilyTaskAssistantViewModel(FamilyTaskWrapper wrapper)
        {
            Model = new FamilyTaskAssistantModel();
            BindingOperations.EnableCollectionSynchronization(_checks, _lock);
            Wrapper = wrapper;
            Title = "Mission Control - Family Task Assistant v." + Assembly.GetExecutingAssembly().GetName().Version;

            Complete = new RelayCommand<Window>(OnComplete);
            EditFamily = new RelayCommand(OnEditFamily);
            Close = new RelayCommand<Window>(OnClose);
            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
            WindowClosed = new RelayCommand<Window>(OnWindowClosed);

            // (Konrad) So we don't know what the central path is. We can toss this into the external
            // event to get that back. when it returns with a result we can update the UI
            Messenger.Default.Register<CentralPathObtained>(this, OnCentralPathObtained);
            Messenger.Default.Register<DocumentClosed>(this, OnDocumentClosed);
            AppCommand.CommunicatorHandler.Request.Make(RequestId.GetCentralPath);
            AppCommand.CommunicatorEvent.Raise();
        }

        private void OnWindowLoaded(Window win)
        {
            Win = win;
        }

        private void OnWindowClosed(Window win)
        {
            // (Konrad) We notify the Communicator View Model that window has closed so View gets set to null and selection is reset.
            Messenger.Default.Send(new TaskAssistantClosedMessage { IsClosed = true });

            // (Konrad) We need to unregister the event handler when window is closed, otherwise it will add another one next time.
            Cleanup();
        }

        private static void OnClose(Window win)
        {
            win.Close();
        }

        private void OnEditFamily()
        {
            Model.EditFamily((FamilyItem)Wrapper.Element);
        }

        private void OnComplete(Window win)
        {
            Model.Submit(Wrapper);
            win.Close();
        }

        private ObservableCollection<CheckWrapper> _checks = new ObservableCollection<CheckWrapper>();
        public ObservableCollection<CheckWrapper> Checks
        {
            get { return _checks; }
            set { _checks = value; RaisePropertyChanged(() => Checks); }
        }

        private FamilyTaskWrapper _wrapper;
        public FamilyTaskWrapper Wrapper
        {
            get { return _wrapper; }
            set { _wrapper = value; RaisePropertyChanged(() => Wrapper); }
        }

        #region Message Handlers

        /// <summary>
        /// Handler for an event of Revit Document getting closed. It's best to close the 
        /// </summary>
        /// <param name="obj"></param>
        private void OnDocumentClosed(DocumentClosed obj)
        {
            Win?.Close();
        }

        /// <summary>
        /// Handler for an event when we request central path from Revit. We need that path, for the
        /// current document to obtain proper settings from MissionControlSetup.
        /// </summary>
        /// <param name="obj"></param>
        private void OnCentralPathObtained(CentralPathObtained obj)
        {
            Model.CentralPath = obj.CentralPath;
            lock (_lock)
            {
                Checks.Clear();
                foreach (var check in Model.CollectChecks((FamilyItem)Wrapper.Element))
                {
                    Checks.Add(check);
                }
            }
        }

        #endregion
    }
}
