using GenerateWPFSample.SharedCode;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GenerateWPFSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GenAIModel? model;
        private CancellationTokenSource? cts;
        private bool isProgressVisible;
        private int maxTextLength;

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var sampleLoadingCts = new CancellationTokenSource();
            var localModelDetails = new SampleNavigationParameters(sampleLoadingCts.Token);
            if (localModelDetails.ShouldWaitForCompletion)
            {
                await localModelDetails.SampleLoadedCompletionSource.Task;
            }

            localModelDetails.RequestWaitForCompletion();
            model = await GenAIModel.CreateAsync(localModelDetails.ModelPath, localModelDetails.PromptTemplate, localModelDetails.CancellationToken);
            maxTextLength = model?.MaxTextLength ?? 0;
            InputTextBox.MaxLength = maxTextLength;
            localModelDetails.NotifyCompletion();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            CancelGeneration();
            model?.Dispose();
        }

        public bool IsProgressVisible
        {
            get => isProgressVisible;
            set
            {
                isProgressVisible = value;
                //DispatcherQueue.TryEnqueue(() =>
                //{
                    OutputProgressBar.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                //});
            }
        }

        public void GenerateText(string topic)
        {
            if (model == null)
            {
                return;
            }

            GenerateTextBlock.Text = string.Empty;
            GenerateButton.Visibility = Visibility.Collapsed;
            StopBtn.Visibility = Visibility.Visible;
            IsProgressVisible = true;
            InputTextBox.IsEnabled = false;

            Task.Run(
                async () =>
                {
                    string systemPrompt = "You generate text based on a user-provided topic. Respond with only the generated content and no extraneous text.";
                    string userPrompt = "Generate text based on the topic: " + topic;

                    cts = new CancellationTokenSource();

                    IsProgressVisible = true;

                    await foreach (var messagePart in model.InferStreaming(systemPrompt, userPrompt, cts.Token))
                    {
                        //DispatcherQueue.TryEnqueue(() =>
                        //{
                            if (isProgressVisible)
                            {
                                StopBtn.Visibility = Visibility.Visible;
                                IsProgressVisible = false;
                            }

                            GenerateTextBlock.Text += messagePart;
                        //});
                    }

                    //DispatcherQueue.TryEnqueue(() =>
                    //{
                        StopBtn.Visibility = Visibility.Collapsed;
                        GenerateButton.Visibility = Visibility.Visible;
                        InputTextBox.IsEnabled = true;
                    //});

                    cts?.Dispose();
                    cts = null;
                });
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.InputTextBox.Text.Length > 0)
            {
                GenerateText(InputTextBox.Text);
            }
        }
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBox)
            {
                if (InputTextBox.Text.Length > 0)
                {
                    GenerateText(InputTextBox.Text);
                }
            }
        }

        private void CancelGeneration()
        {
            StopBtn.Visibility = Visibility.Collapsed;
            IsProgressVisible = false;
            GenerateButton.Visibility = Visibility.Visible;
            InputTextBox.IsEnabled = true;
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            CancelGeneration();
        }

        private void InputBox_Changed(object sender, TextChangedEventArgs e)
        {
            var inputLength = InputTextBox.Text.Length;
            if (inputLength > 0)
            {
                if (inputLength >= maxTextLength)
                {
                    System.Diagnostics.Debug.WriteLine($"{inputLength} of {maxTextLength}. Max characters reached.");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"{inputLength} of {maxTextLength}");
                }

                GenerateButton.IsEnabled = inputLength <= maxTextLength;
            }
            else
            {
                GenerateButton.IsEnabled = false;
            }
        }
    }
}