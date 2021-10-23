using DifferenceUtility.Net;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Sample.Assets;

namespace Sample.MvvmCross.Core
{
    public class MainViewModel : MvxViewModel
    {
        private bool _isLoading;
        private readonly PersonDiffCallback _personDiffCallback = new();

        public IMvxCommand ClearDataButtonClickCommand { get; set; }

        public MvxObservableCollection<PersonModel> Data { get; set; }

        public IMvxCommand LoadDataButtonClickCommand { get; set; }

        private void ClearDataButton_Click()
        {
            if (Data.Count > 0)
                Data.Clear();
        }

        private void LoadDataButton_Click()
        {
            if (_isLoading)
                return;

            _isLoading = true;

            // Request collection of people.
            API.GetPeopleAsync().ContinueWith((task) =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    // Calculate the difference between the new data, and the existing data.
                    var diffResult = DiffUtil.CalculateDiff(Data, task.Result, _personDiffCallback);

                    InvokeOnMainThread(() =>
                    {
                        // Apply the changes to the data.
                        diffResult.Apply(Data);
                    });
                }

                _isLoading = false;
            });
        }

        public override void Prepare()
        {
            base.Prepare();

            Data = new MvxObservableCollection<PersonModel>();

            ClearDataButtonClickCommand = new MvxCommand(ClearDataButton_Click);
            LoadDataButtonClickCommand = new MvxCommand(LoadDataButton_Click);
        }
    }
}
