﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TaskList.Model;

namespace TaskList.ViewModel
{
    public class NonUrgentUnimportantTasksVM : ObservableObject
    {
        private ObservableCollection<TheTask> _tempSwapCollection, _items = new ObservableCollection<TheTask>();
        public ObservableCollection<TheTask> TaskCollection
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    RaisePropertyChangedEvent(nameof(TaskCollection));
                }
            }
        }

        private TheTask _current = null;
        public TheTask CurrentTask
        {
            get { return _current; }
            set
            {
                _current = value;
                RaisePropertyChangedEvent(nameof(CurrentTask));
            }
        }

        public ICommand AddTask
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    _items.Add(new TheTask());
                    RaisePropertyChangedEvent(nameof(TaskCollection));
                });
            }
        }

        public ICommand CompleteTask
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    for (int i = 0; i < TaskCollection.Count; i++)
                    {
                        if (CurrentTask == null) return;
                        _tempSwapCollection = new ObservableCollection<TheTask>();
                        if (_items[i].Content == CurrentTask.Content)
                        {
                            _items[i].CloseTask();
                            _items[i].SetContent((_items[i].Content + " gg wp"));
                            _items.ToList<TheTask>().ForEach((temp) => _tempSwapCollection.Add(temp));
                            _items.Clear();
                            _tempSwapCollection.ToList<TheTask>().ForEach((temp) => _items.Add(temp));
                            CurrentTask = _items[i];
                            break;
                        }
                    }
                    RaisePropertyChangedEvent(nameof(TaskCollection));
                });
            }
        }
    }
}
