﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Serialization;
using TaskList.Model;
using TaskList.ToolKit.Command;
using TaskList.ToolKit.ViewModel;
using TaskList.View;
using TaskList.ViewModel.Interfaces;

namespace TaskList.ViewModel.Abstract
{
    public abstract class TaskType : ObservableObject, ITaskType
    {
        private string _fileName, _fileDeletesName;
        private readonly string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\TaskList";
        private ObservableCollection<TheTask> _ms, _items = new ObservableCollection<TheTask>();

        ~TaskType()
        {
            SerializeAndWriteCollection(_items);
        }

        [XmlArray("Collection"), XmlArrayItem("Item")]
        public ObservableCollection<TheTask> TaskCollection
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChangedEvent(nameof(TaskCollection));
            }
        }

        public bool EnabledProperty => CurrentTask != null;

        private TheTask _current;
        public TheTask CurrentTask
        {
            get => _current;
            set
            {
                _current = value;
                OnPropertyChangedEvent(nameof(CurrentTask));
                OnPropertyChangedEvent(nameof(EnabledProperty));
            }
        }

        private DelegateCommand _addTask;
        public ICommand AddTask
        {
            get
            {
                return _addTask ?? (_addTask = new DelegateCommand((o) =>
                {
                    TaskWindow temp = new TaskWindow();
                    TaskWindowViewModel twvm = (TaskWindowViewModel)temp.DataContext;

                    if (temp.ShowDialog() == true)
                    {
                        if (twvm.Content.Trim() != "")
                        {
                            File.AppendAllText(_filePath + @"\logs.txt", new string('.', 25) + "" + new string('.', 25) + Environment.NewLine);
                            _items.Add(new TheTask(twvm.Content));
                            File.AppendAllText(_filePath + @"\logs.txt", new string('.', 80) + Environment.NewLine);
                        }
                    }
                    OnPropertyChangedEvent(nameof(TaskCollection));
                }));
            }
        }

        private DelegateCommand _completeTask;
        public ICommand CompleteTask
        {
            get
            {
                return _completeTask ?? (_completeTask = new DelegateCommand((o) =>
                {
                    int i = _items.IndexOf(CurrentTask);
                    if (i != -1)
                    {
                        if (CurrentTask.Status == "Выполнено") return;
                        _items[i].SetStatus(1);
                        UpdateCollection();
                        CurrentTask = _items[i];
                    }
                    OnPropertyChangedEvent(nameof(TaskCollection));
                }));
            }
        }

        private DelegateCommand _deleteTask;
        public ICommand DeleteTask
        {
            get
            {
                return _deleteTask ?? (_deleteTask = new DelegateCommand(o =>
                {
                    int i = _items.IndexOf(CurrentTask);
                    if (i != -1)
                    {
                        File.AppendAllText(_filePath + _fileDeletesName, String.Format(DateTime.Now + "\tДело \"{0}\" со статусом \"{1}\", начатое {2} удалено из списка." + Environment.NewLine, _items[i].Content, _items[i].Status, _items[i].StartDate));
                        _items[i].SetStatus(2);
                        _items[i].DeleteTask();
                        _items.RemoveAt(i);
                    }
                }));
            }
        }

        private DelegateCommand _editTask;
        public ICommand EditTask
        {
            get
            {
                return _editTask ?? (_editTask = new DelegateCommand((o) =>
               {
                   TaskWindow temp = new TaskWindow();
                   TaskWindowViewModel twvm = (TaskWindowViewModel)temp.DataContext;
                   twvm.Content = CurrentTask.Content;

                   if (temp.ShowDialog() == true)
                   {
                       if (twvm.Content.Trim() != "")
                       {
                           int i = _items.IndexOf(CurrentTask);
                           if (i != -1)
                           {
                               _items[i].SetContent(twvm.Content);
                               UpdateCollection();
                               CurrentTask = _items[i];
                           }
                       }
                   }
                   OnPropertyChangedEvent(nameof(TaskCollection));
               }));
            }
        }

        protected void InitTaskType(string nameOfXmlFile, string nameOfXmlFileRemoteTasks)
        {
            _fileName = nameOfXmlFile;
            _fileDeletesName = nameOfXmlFileRemoteTasks;
            LoadTaskFromFile();
        }

        private void LoadTaskFromFile()
        {
            if (!Directory.Exists(_filePath)) { Directory.CreateDirectory(_filePath); return; }
            if (!File.Exists(_filePath + _fileName)) { return; }
            if (!File.Exists(_filePath + _fileDeletesName)) { File.Create(_filePath + _fileDeletesName); }
            ReadAndDeserializeCollection(ref _items);
        }

        private void UpdateCollection()
        {
            _ms = new ObservableCollection<TheTask>();
            _items.ToList().ForEach(temp => _ms.Add(temp));
            _items.Clear();
            _ms.ToList().ForEach(temp => _items.Add(temp));
        }

        #region (De)SerializeCollection
        private void SerializeAndWriteCollection(ObservableCollection<TheTask> serializeCollection)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ObservableCollection<TheTask>));
            TextWriter stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, serializeCollection);
            File.WriteAllText(_filePath + _fileName, stringWriter.ToString());
        }

        private void ReadAndDeserializeCollection(ref ObservableCollection<TheTask> deserializeCollection)
        {
            if (deserializeCollection == null) return;
            string serializedData = File.ReadAllText(_filePath + _fileName);
            var xmlSerializer = new XmlSerializer(typeof(ObservableCollection<TheTask>));
            var stringReader = new StringReader(serializedData);
            deserializeCollection = (ObservableCollection<TheTask>)xmlSerializer.Deserialize(stringReader);
        }
        #endregion
    }
}