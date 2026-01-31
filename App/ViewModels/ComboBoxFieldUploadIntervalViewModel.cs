using System.Collections.ObjectModel;

namespace App.ViewModels;

public class ComboBoxFieldUploadIntervalViewModel(ScheduleOption selectedOption, string label, ObservableCollection<ScheduleOption> options)
  : ComboBoxFieldViewModel<ScheduleOption>(selectedOption, label, options);
