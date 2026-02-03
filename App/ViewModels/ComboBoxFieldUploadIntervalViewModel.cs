using System.Collections.ObjectModel;

namespace App.ViewModels;

public class ComboBoxFieldUploadIntervalViewModel(ScheduleOption selectedOption, ObservableCollection<ScheduleOption> options, string label = "")
  : ComboBoxFieldViewModel<ScheduleOption>(selectedOption, options, label);
