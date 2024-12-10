﻿using Microsoft.Maui.Controls;

namespace MauiApp1
{
    public partial class JobDetailsPage : ContentPage
    {
        private readonly ApiService _apiService = new ApiService();

        public JobDetailsPage(Job selectedJob)
        {
            InitializeComponent();
            BindingContext = selectedJob; // Set initial data for quick display
            LoadJobDetails(selectedJob.Id); // Load detailed data asynchronously
        }

        // Load detailed job data from the API
        private async void LoadJobDetails(string jobId)
        {
            try
            {
                var jobDetailsResponse = await _apiService.GetJobDetailsAsync(jobId);

                if (jobDetailsResponse != null && jobDetailsResponse.Status == "ok")
                {
                    var detailedJob = jobDetailsResponse.Data;

                    // Combine basic `Job` data and detailed `JobDetails`
                    if (BindingContext is Job basicJob)
                    {
                        detailedJob.Id = basicJob.Id; // Ensure continuity
                        detailedJob.JobTitle = basicJob.JobTitle;
                        detailedJob.CompanyName = basicJob.CompanyName;
                        detailedJob.Geography = basicJob.Geography;
                    }

                    BindingContext = detailedJob;

                    // Update WebView Source
                    ContentWebView.Source = new HtmlWebViewSource
                    {
                        Html = detailedJob.Content ?? "<p>No content available.</p>"
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading job details: {ex.Message}");
                await DisplayAlert("Error", "An error occurred while fetching job details.", "OK");
            }
        }




        // Handle application URL button click
        private async void OnOpenApplicationUrlClicked(object sender, EventArgs e)
        {
            if (BindingContext is Job job && !string.IsNullOrWhiteSpace(job.ApplicationUrl))
            {
                try
                {
                    await Browser.OpenAsync(job.ApplicationUrl, BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening URL: {ex.Message}");
                    await DisplayAlert("Error", "Unable to open the application link.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Info", "No application link available for this job.", "OK");
            }
        }

        // Handle "Add to Favorites" action
        private void AddToFavorites(object sender, EventArgs e)
        {
            if (BindingContext is Job job)
            {
                // Add to favorites list (global list)
                if (!App.FavoritesList.Contains(job))
                {
                    App.FavoritesList.Add(job);
                    DisplayAlert("Tilføjet til favoriter", $"{job.JobTitle} er blevet tilføjet til dine favoriter.", "OK");
                }
                else
                {
                    DisplayAlert("Allereder i favoriter", $"{job.JobTitle} er allerede i din favoriter.", "OK");
                }
            }
        }

        private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            var currentTheme = Application.Current.RequestedTheme;

            // Adjust background color based on the current theme
            string backgroundColor = currentTheme == AppTheme.Dark ? "#333333" : "#FFFFFF";
            string textColor = currentTheme == AppTheme.Dark ? "#FFFFFF" : "#000000";

            // Inject JavaScript to update content background and text color
            string js = $"document.body.style.backgroundColor = '{backgroundColor}'; " +
                        $"document.body.style.color = '{textColor}';";
            ContentWebView.Eval(js);
        }

    }
}
