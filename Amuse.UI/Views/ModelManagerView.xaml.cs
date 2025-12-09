using Amuse.UI.Commands;
using Amuse.UI.Dialogs;
using Amuse.UI.Helpers;
using Amuse.UI.Models;
using Amuse.UI.Models.FeatureExtractor;
using Amuse.UI.Models.StableDiffusion;
using Amuse.UI.Models.Upscale;
using Amuse.UI.Services;
using Microsoft.Extensions.Logging;
using OnnxStack.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;


namespace Amuse.UI.Views
{
    /// <summary>
    /// Interaction logic for ModelManagerView.xaml
    /// </summary>
    public partial class ModelManagerView : SettingsViewBase, INavigatable
    {
        private readonly IDialogService _dialogService;
        private readonly IModelDownloadService _modelDownloadService;

        private ModelTemplateViewModel _selectedModelTemplate;
        private ICollectionView _modelTemplateCollectionView;
        private string _modelTemplateFilterSearch;
        private string _modelTemplateFilterType;
        private List<string> _modelTemplateFilterTypes;
        private string _modelTemplateFilterTag;
        private List<string> _modelTemplateFilterTags;
        private Device _modelTemplateFilterDevice;
        private bool _modelTemplateFilterOnlineTemplates = true;
        private bool _modelTemplateFilterInstalledTemplates = true;
        private bool _modelTemplateFilterFixedTemplates = true;
        private string _modelTemplateSortProperty;
        private ListSortDirection _modelTemplateSortDirection;
        private LayoutViewType _modelTemplateLayoutView = LayoutViewType.TileSmall;
        private ModelTemplateCategory? _modelTemplateCategory;
        private int _selectedCategory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelManagerView"/> class.
        /// </summary>
        public ModelManagerView()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _dialogService = App.GetService<IDialogService>();
                _modelDownloadService = App.GetService<IModelDownloadService>();
            }

            UpdateModelSettingsCommand = new AsyncRelayCommand(UpdateModelAdvanced);
            OpenModelDirectoryCommand = new AsyncRelayCommand(OpenModelDirectory);
            RemoveModelCommand = new AsyncRelayCommand(RemoveModel, CanExecuteRemoveModel);
            DeleteModelCommand = new AsyncRelayCommand(DeleteModel, CanExecuteDeleteModel);
            InstallModelCommand = new AsyncRelayCommand(InstallModel);
            UninstallModelCommand = new AsyncRelayCommand(UninstallModel);
            DownloadModelCommand = new AsyncRelayCommand(DownloadModel);
            DownloadModelCancelCommand = new AsyncRelayCommand(DownloadModelCancel);
            ModelTemplateFilterTagCommand = new AsyncRelayCommand<string>(ModelTemplateFilterSetTag);
            ModelTemplateFilterResetCommand = new AsyncRelayCommand(ModelTemplateFilterReset);
            UpdateModelMetadataCommand = new AsyncRelayCommand(UpdateModelMetadata);
            ViewModelMetadataCommand = new AsyncRelayCommand(ViewModelMetadata);
            ModelTemplateLayoutCommand = new AsyncRelayCommand<LayoutViewType>(ModelTemplateLayout);
            UpdateModelCommand = new AsyncRelayCommand(DownloadModel);

            ImportModelTemplateCommand = new AsyncRelayCommand(ImportModelTemplate);
            ExportModelTemplateCommand = new AsyncRelayCommand(ExportModelTemplate, CanExportModelTemplate);
            InitializeComponent();
        }

        public static readonly DependencyProperty DefaultExecutionDeviceProperty =
            DependencyProperty.Register(nameof(DefaultExecutionDevice), typeof(Device), typeof(ModelManagerView), new PropertyMetadata<ModelManagerView, Device>((c, o, n) => c.OnDefaultDeviceChanged(n)));

        public AsyncRelayCommand UpdateModelSettingsCommand { get; }
        public AsyncRelayCommand OpenModelDirectoryCommand { get; }
        public AsyncRelayCommand RemoveModelCommand { get; }
        public AsyncRelayCommand DeleteModelCommand { get; }
        public AsyncRelayCommand InstallModelCommand { get; }
        public AsyncRelayCommand UninstallModelCommand { get; }
        public AsyncRelayCommand DownloadModelCommand { get; }
        public AsyncRelayCommand DownloadModelCancelCommand { get; }
        public AsyncRelayCommand<string> ModelTemplateFilterTagCommand { get; }
        public AsyncRelayCommand ModelTemplateFilterResetCommand { get; }
        public AsyncRelayCommand<LayoutViewType> ModelTemplateLayoutCommand { get; }
        public AsyncRelayCommand ImportModelTemplateCommand { get; }
        public AsyncRelayCommand ExportModelTemplateCommand { get; }
        public AsyncRelayCommand UpdateModelMetadataCommand { get; }
        public AsyncRelayCommand ViewModelMetadataCommand { get; }
        public AsyncRelayCommand UpdateModelCommand { get; }

        public Device DefaultExecutionDevice
        {
            get { return (Device)GetValue(DefaultExecutionDeviceProperty); }
            set { SetValue(DefaultExecutionDeviceProperty, value); }
        }

        public ModelTemplateViewModel SelectedModelTemplate
        {
            get { return _selectedModelTemplate; }
            set { _selectedModelTemplate = value; NotifyPropertyChanged(); }
        }

        public ICollectionView ModelTemplateCollectionView
        {
            get { return _modelTemplateCollectionView; }
            set { _modelTemplateCollectionView = value; NotifyPropertyChanged(); }
        }

        public string ModelTemplateFilterSearch
        {
            get { return _modelTemplateFilterSearch; }
            set { _modelTemplateFilterSearch = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public string ModelTemplateFilterType
        {
            get { return _modelTemplateFilterType; }
            set { _modelTemplateFilterType = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public List<string> ModelTemplateFilterTypes
        {
            get { return _modelTemplateFilterTypes; }
            set { _modelTemplateFilterTypes = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public string ModelTemplateFilterTag
        {
            get { return _modelTemplateFilterTag; }
            set { _modelTemplateFilterTag = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public List<string> ModelTemplateFilterTags
        {
            get { return _modelTemplateFilterTags; }
            set { _modelTemplateFilterTags = value; NotifyPropertyChanged(); }
        }

        public bool ModelTemplateFilterOnlineTemplates
        {
            get { return _modelTemplateFilterOnlineTemplates; }
            set { _modelTemplateFilterOnlineTemplates = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public bool ModelTemplateFilterInstalledTemplates
        {
            get { return _modelTemplateFilterInstalledTemplates; }
            set { _modelTemplateFilterInstalledTemplates = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public bool ModelTemplateFilterFixedTemplates
        {
            get { return _modelTemplateFilterFixedTemplates; }
            set { _modelTemplateFilterFixedTemplates = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public Device ModelTemplateFilterDevice
        {
            get { return _modelTemplateFilterDevice; }
            set { _modelTemplateFilterDevice = value; NotifyPropertyChanged(); ModelTemplateRefresh(); }
        }

        public string ModelTemplateSortProperty
        {
            get { return _modelTemplateSortProperty; }
            set { _modelTemplateSortProperty = value; NotifyPropertyChanged(); ModelTemplateSort(); }
        }

        public ListSortDirection ModelTemplateSortDirection
        {
            get { return _modelTemplateSortDirection; }
            set { _modelTemplateSortDirection = value; NotifyPropertyChanged(); ModelTemplateSort(); }
        }

        public LayoutViewType ModelTemplateLayoutView
        {
            get { return _modelTemplateLayoutView; }
            set { _modelTemplateLayoutView = value; NotifyPropertyChanged(); }
        }




        public int SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                _selectedCategory = value;
                _modelTemplateFilterFixedTemplates = false;
                _modelTemplateFilterOnlineTemplates = true;
                if (_selectedCategory == 0)
                    _modelTemplateCategory = null;
                else if (_selectedCategory == 1)
                    _modelTemplateCategory = ModelTemplateCategory.StableDiffusion;
                else if (_selectedCategory == 2)
                    _modelTemplateCategory = ModelTemplateCategory.ControlNet;
                else if (_selectedCategory == 3)
                    _modelTemplateCategory = ModelTemplateCategory.Upscaler;
                else if (_selectedCategory == 4)
                    _modelTemplateCategory = ModelTemplateCategory.FeatureExtractor;
                else if (_selectedCategory == 5)
                {
                    _modelTemplateCategory = null;
                    _modelTemplateFilterInstalledTemplates = false;
                    _modelTemplateFilterFixedTemplates = true;
                    _modelTemplateFilterOnlineTemplates = false;
                }
                ModelTemplateCollectionView.Refresh();
            }
        }



        /// <summary>
        /// Called when Settings changed.
        /// </summary>
        /// <returns></returns>
        protected override Task OnSettingsChanged()
        {
            ModelTemplateFilterDevice = Settings.DefaultExecutionDevice;
            InitializeTemplates();
            return base.OnSettingsChanged();
        }


        /// <summary>
        /// Called when DeviceInfo changed.
        /// </summary>
        /// <param name="deviceInfo">The device information.</param>
        /// <returns></returns>
        private Task OnDefaultDeviceChanged(Device device)
        {
            ModelTemplateFilterDevice = device;
            return Task.CompletedTask;
        }


        public Task NavigateAsync(IImageResult imageResult)
        {
            throw new NotImplementedException();
        }


        public Task NavigateAsync(IVideoResult videoResult)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Removes the model.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task RemoveModel()
        {
            if (SelectedModelTemplate == null)
                return;

            if (!await DialogService.ShowMessageDialogAsync("Remove Model", "Are you sure you want to remove this model template?", MessageDialog.MessageDialogType.YesNo, MessageDialog.MessageBoxIconType.Question, MessageDialog.MessageBoxStyleType.Info))
                return;

            if (SelectedModelTemplate.IsProtected)
                return;

            await DeleteModelCache(SelectedModelTemplate.Id);
            await (SelectedModelTemplate.Category switch
            {
                ModelTemplateCategory.Upscaler => RemoveUpscaleModel(SelectedModelTemplate),
                ModelTemplateCategory.ControlNet => RemoveControlNetModel(SelectedModelTemplate),
                ModelTemplateCategory.StableDiffusion => RemoveStableDiffusionModel(SelectedModelTemplate),
                ModelTemplateCategory.FeatureExtractor => RemoveFeatureExtractorModel(SelectedModelTemplate),
                _ => throw new NotImplementedException()
            });
        }


        private bool CanExecuteRemoveModel()
        {
            return SelectedModelTemplate is not null
                && SelectedModelTemplate.IsProtected == false
                && SelectedModelTemplate.IsFixedInstall == false;
        }


        /// <summary>
        /// Deletes the model.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private async Task DeleteModel()
        {
            if (SelectedModelTemplate == null)
                return;

            if (!await DialogService.ShowMessageDialogAsync("Delete Model", "Are you sure you want to delete all model files?", MessageDialog.MessageDialogType.YesNo, MessageDialog.MessageBoxIconType.Question, MessageDialog.MessageBoxStyleType.Info))
                return;

            try
            {
                var modelDirectory = GetModelDirectory(SelectedModelTemplate);
                if (Directory.Exists(modelDirectory))
                {
                    await Task.Run(() => Directory.Delete(modelDirectory, true));
                }
                await UninstallModel();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorMessageAsync("Delete Failed", $"Failed to delete model files\n{ex.Message}");
            }
        }


        private bool CanExecuteDeleteModel()
        {
            return SelectedModelTemplate is not null
                && SelectedModelTemplate.IsFixedInstall == false;
        }


        private Task OpenModelDirectory()
        {
            var modelDirectory = GetModelDirectory(SelectedModelTemplate);
            if (Directory.Exists(modelDirectory))
            {
                Utils.NavigateToUrl(modelDirectory);
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// Installs the model.
        /// </summary>
        /// <returns></returns>
        private Task InstallModel()
        {
            if (SelectedModelTemplate == null)
                return Task.CompletedTask;

            return SelectedModelTemplate.Category switch
            {
                ModelTemplateCategory.Upscaler => InstallUpscaleModel(SelectedModelTemplate),
                ModelTemplateCategory.ControlNet => InstallControlNetModel(SelectedModelTemplate),
                ModelTemplateCategory.StableDiffusion => InstallStableDiffusionModel(SelectedModelTemplate),
                ModelTemplateCategory.FeatureExtractor => InstallFeatureExtractorModel(SelectedModelTemplate),
                _ => throw new NotImplementedException()
            };
        }


        /// <summary>
        /// Uninstalls the model.
        /// </summary>
        /// <returns></returns>
        private Task UninstallModel()
        {
            if (SelectedModelTemplate == null)
                return Task.CompletedTask;

            return SelectedModelTemplate.Category switch
            {
                ModelTemplateCategory.Upscaler => UninstallUpscaleModel(SelectedModelTemplate),
                ModelTemplateCategory.ControlNet => UninstallControlNetModel(SelectedModelTemplate),
                ModelTemplateCategory.StableDiffusion => UninstallStableDiffusionModel(SelectedModelTemplate),
                ModelTemplateCategory.FeatureExtractor => UninstallFeatureExtractorModel(SelectedModelTemplate),
                _ => throw new NotImplementedException()
            };
        }


        /// <summary>
        /// Updates the model advanced.
        /// </summary>
        /// <returns></returns>
        private Task UpdateModelAdvanced()
        {
            return SelectedModelTemplate.Category switch
            {
                ModelTemplateCategory.Upscaler => UpdateUpscaleModel(SelectedModelTemplate),
                ModelTemplateCategory.ControlNet => UpdateControlNetModel(SelectedModelTemplate),
                ModelTemplateCategory.StableDiffusion => UpdateStableDiffusionModel(SelectedModelTemplate),
                ModelTemplateCategory.FeatureExtractor => UpdateFeatureExtractorModel(SelectedModelTemplate),
                _ => throw new NotImplementedException()
            };
        }


        /// <summary>
        /// Downloads the model.
        /// </summary>
        /// <returns></returns>
        private async Task DownloadModel()
        {
            if (SelectedModelTemplate == null)
                return;

            var liceneDialog = _dialogService.GetDialog<ModelLicenceDialog>();
            if (await liceneDialog.ShowDialogAsync(SelectedModelTemplate))
            {
                await _modelDownloadService.QueueDownloadAsync(SelectedModelTemplate);
            }
        }


        /// <summary>
        /// Cancels the model download
        /// </summary>
        /// <returns></returns>
        private Task DownloadModelCancel()
        {
            if (_selectedModelTemplate.IsQueued)
            {
                SelectedModelTemplate.IsQueued = false;
                SelectedModelTemplate.IsDownloading = false;
            }
            _selectedModelTemplate?.CancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }


        /// <summary>
        /// Update the ModelTemplate layout view
        /// </summary>
        /// <param name="layoutViewType">Type of the layout view.</param>
        /// <returns></returns>
        private Task ModelTemplateLayout(LayoutViewType layoutViewType)
        {
            ModelTemplateLayoutView = layoutViewType;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Updates the model metadata.
        /// </summary>
        private async Task UpdateModelMetadata()
        {
            var updateMetadataDialog = DialogService.GetDialog<UpdateModelMetadataDialog>();
            if (await updateMetadataDialog.ShowDialogAsync(_selectedModelTemplate))
            {
                await SaveAsync();
            }
        }


        /// <summary>
        /// Views the model metadata.
        /// </summary>
        /// <returns></returns>
        private async Task ViewModelMetadata()
        {
            var viewMetadataDialog = DialogService.GetDialog<ViewModelMetadataDialog>();
            await viewMetadataDialog.ShowDialogAsync(_selectedModelTemplate);
        }


        private async Task SaveAndRefresh()
        {
            App.UIInvoke(ModelTemplateRefresh);
            await App.UIInvokeAsync(SaveAsync);
        }


        #region ModelTemplate


        /// <summary>
        /// Initializes the templates.
        /// </summary>
        private void InitializeTemplates()
        {
            if (ModelTemplateCollectionView != null)
                ModelTemplateCollectionView.CollectionChanged -= ModelTemplateCollectionView_CollectionChanged;

            ModelTemplateFilterTypes = Settings.Templates
                 .Where(x => x.Group == ModelTemplateGroup.Fixed)
                 .OrderBy(x => x.Category)
                 .Select(x => x.Template)
                 .ToList();
            ModelTemplateFilterTypes.Insert(0, "All");
            ModelTemplateFilterType = "All";

            ModelTemplateFilterTags = Settings.Templates
                 .Where(x => !x.Tags.IsNullOrEmpty())
                 .SelectMany(x => x.Tags)
                 .Distinct()
                 .OrderBy(x => x)
                 .ToList();
            ModelTemplateFilterTags.Insert(0, "All");
            ModelTemplateFilterTag = "All";


            ModelTemplateCollectionView = new ListCollectionView(Settings.Templates);
            ModelTemplateCollectionView.GroupDescriptions.Add(new PropertyGroupDescription("GroupHeader", new Converters.EnumDescriptionConverter()));
            ModelTemplateSort();
            ModelTemplateCollectionView.Filter = ModelTemplateFilter;
            ModelTemplateCollectionView.MoveCurrentToFirst();
            SelectedModelTemplate = (ModelTemplateViewModel)ModelTemplateCollectionView.CurrentItem;
            ModelTemplateCollectionView.CollectionChanged += ModelTemplateCollectionView_CollectionChanged;
        }


        /// <summary>
        /// Handles the CollectionChanged event of the ModelTemplateCollectionView.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void ModelTemplateCollectionView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!ModelTemplateCollectionView.Contains(SelectedModelTemplate))
            {
                // If the selected item no longer exists(filtered) select first
                ModelTemplateCollectionView.MoveCurrentToFirst();
                SelectedModelTemplate = (ModelTemplateViewModel)ModelTemplateCollectionView.CurrentItem;
            }
        }


        /// <summary>
        /// Refresh the Models the template view.
        /// </summary>
        private void ModelTemplateRefresh()
        {
            if (ModelTemplateCollectionView is null)
                return;
            ModelTemplateCollectionView.Refresh();
        }



        /// <summary>
        /// Models template filter.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool ModelTemplateFilter(object obj)
        {
            if (obj is not ModelTemplateViewModel template)
                return false;

            return (string.IsNullOrEmpty(_modelTemplateFilterSearch) || template.Name.Contains(_modelTemplateFilterSearch, StringComparison.OrdinalIgnoreCase))
                && (_modelTemplateFilterTag == "All" || (!template.Tags.IsNullOrEmpty() && template.Tags.Contains(_modelTemplateFilterTag, StringComparer.OrdinalIgnoreCase)))
                && (_modelTemplateFilterType == "All" || _modelTemplateFilterType == template.Template)
                 && (_modelTemplateCategory == null || _modelTemplateCategory == template.Category)
                && ((_modelTemplateFilterInstalledTemplates && template.IsInstalled)
                    || (_modelTemplateFilterOnlineTemplates && template.Group == ModelTemplateGroup.Online)
                    || (_modelTemplateFilterFixedTemplates && template.Group == ModelTemplateGroup.Fixed));
        }


        /// <summary>
        /// Reset the filters
        /// </summary>
        /// <returns></returns>
        private Task ModelTemplateFilterReset()
        {
            ModelTemplateFilterTag = "All";
            ModelTemplateFilterType = "All";
            ModelTemplateFilterSearch = null;
            ModelTemplateFilterDevice = Settings.DefaultExecutionDevice;
            ModelTemplateFilterFixedTemplates = true;
            ModelTemplateFilterOnlineTemplates = true;
            ModelTemplateFilterInstalledTemplates = true;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Set the selected model filter tag
        /// </summary>
        /// <param name="selectedTag">The selected tag.</param>
        /// <returns></returns>
        private Task ModelTemplateFilterSetTag(string selectedTag)
        {
            ModelTemplateFilterTag = selectedTag;
            return Task.CompletedTask;
        }


        /// <summary>
        /// Sort the model templates
        /// </summary>
        private void ModelTemplateSort()
        {
            if (ModelTemplateCollectionView is null)
                return;

            ModelTemplateCollectionView.SortDescriptions.Clear();
            if (ModelTemplateSortProperty == "Status")
            {
                var param1 = ModelTemplateSortDirection == ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
                var param2 = ModelTemplateSortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription("IsUserTemplate", param2));
                ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription("Category", param1));
                ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription("IsInstalled", param2));
                ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription("Rank", param1));
                ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription("Name", param1));
                return;
            }
            ModelTemplateCollectionView.SortDescriptions.Add(new SortDescription(ModelTemplateSortProperty, ModelTemplateSortDirection));
        }


        /// <summary>
        /// Imports a model template.
        /// </summary>
        private async Task ImportModelTemplate()
        {
            var selectedFilename = await DialogService.OpenFileDialogAsync("Import Template", Settings.DirectoryModel, filter: "Template Files|*_template.json;");
            if (string.IsNullOrEmpty(selectedFilename))
                return;

            try
            {
                using (var reader = File.OpenRead(selectedFilename))
                {
                    var serializerOptions = new JsonSerializerOptions();
                    serializerOptions.Converters.Add(new JsonStringEnumConverter());
                    var template = await JsonSerializer.DeserializeAsync<ModelTemplateViewModel>(reader, serializerOptions);
                    var existingTempate = Settings.Templates.FirstOrDefault(x => x.Id == template.Id);
                    if (existingTempate is not null)
                    {
                        await DialogService.ShowErrorMessageAsync("Template Exists", $"Template with this Id already exists");
                        return;
                    }

                    Settings.Templates.Add(template);
                    await Settings.SaveAsync();
                    await Task.Delay(1000);
                    SelectedModelTemplate = template;
                    ModelTemplateRefresh();
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorMessageAsync("Import Failed", $"Failed to read template file!\n\n{ex.Message}");
            }
        }


        /// <summary>
        /// Exports the SelectedModelTemplate.
        /// </summary>
        private async Task ExportModelTemplate()
        {
            var selectedFilename = await DialogService.SaveFileDialogAsync("Export Template", $"{SelectedModelTemplate.Name.Replace(" ", "-")}_template.json", Settings.DirectoryModel, filter: "Template Files|*_template.json;");
            if (string.IsNullOrEmpty(selectedFilename))
                return;
            try
            {
                using (var writer = File.OpenWrite(selectedFilename))
                {
                    var serializerOptions = new JsonSerializerOptions();
                    serializerOptions.WriteIndented = true;
                    serializerOptions.Converters.Add(new JsonStringEnumConverter());
                    await JsonSerializer.SerializeAsync(writer, SelectedModelTemplate, serializerOptions);
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorMessageAsync("Export Failed", $"Failed to write template file!\n\n{ex.Message}");
            }
        }


        /// <summary>
        /// Can SelectedModelTemplate be exported
        /// </summary>
        private bool CanExportModelTemplate()
        {
            if (SelectedModelTemplate is null)
                return false;

            return SelectedModelTemplate.Group != ModelTemplateGroup.Fixed;
        }


        /// <summary>
        /// Deletes the model cache.
        /// </summary>
        /// <param name="modelId">The model identifier.</param>
        private Task DeleteModelCache(Guid modelId)
        {
            try
            {
                var cacheDirectory = Path.Combine(App.CacheDirectory, modelId.ToString());
                if (Directory.Exists(cacheDirectory))
                    Directory.Delete(cacheDirectory, true);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to delete model image cache\n\n{ex.Message}");
            }
            return Task.CompletedTask;
        }


        private string GetModelDirectory(ModelTemplateViewModel modelTemplate)
        {
            var modelDirectory = string.Empty;
            if (modelTemplate.Category == ModelTemplateCategory.Upscaler)
            {
                var modelsetConfig = Settings.UpscaleModelSets
                     .Where(x => x.Id == modelTemplate.Id)
                     .Select(x => x.ModelSet)
                     .FirstOrDefault();
                modelDirectory = Path.GetDirectoryName(modelsetConfig.OnnxModelPath);
            }
            else if (modelTemplate.Category == ModelTemplateCategory.ControlNet)
            {
                var modelsetConfig = Settings.ControlNetModelSets
                     .Where(x => x.Id == modelTemplate.Id)
                     .Select(x => x.ModelSet)
                     .FirstOrDefault();
                modelDirectory = Path.GetDirectoryName(modelsetConfig.OnnxModelPath);
            }
            else if (modelTemplate.Category == ModelTemplateCategory.StableDiffusion)
            {
                var modelsetConfig = Settings.StableDiffusionModelSets
                     .Where(x => x.Id == modelTemplate.Id)
                     .Select(x => x.ModelSet.UnetConfig)
                     .FirstOrDefault();
                modelDirectory = Path.GetDirectoryName(modelsetConfig.OnnxModelPath);
                modelDirectory = Directory.GetParent(modelDirectory).FullName;
            }
            else if (modelTemplate.Category == ModelTemplateCategory.FeatureExtractor)
            {
                var modelsetConfig = Settings.FeatureExtractorModelSets
                    .Where(x => x.Id == modelTemplate.Id)
                    .Select(x => x.ModelSet)
                    .FirstOrDefault();
                modelDirectory = Path.GetDirectoryName(modelsetConfig.OnnxModelPath);
            }
            return modelDirectory;
        }


        #endregion

        #region StableDiffusion Model


        /// <summary>
        /// Installs the stable diffusion model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task InstallStableDiffusionModel(ModelTemplateViewModel modelTemplate)
        {
            var addModelDialog = DialogService.GetDialog<AddModelDialog>();
            if (!await addModelDialog.ShowDialogAsync(modelTemplate))
                return; // User Canceled


            var modelSetResult = addModelDialog.ModelSetResult;
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
            {
                SelectedModelTemplate = AddTemplate(modelSetResult.Name, modelTemplate);
                SelectedModelTemplate.Repository = addModelDialog.ModelFolder;
                SelectedModelTemplate.StableDiffusionTemplate = modelTemplate.StableDiffusionTemplate with { };
                Settings.StableDiffusionModelSets.Add(new StableDiffusionModelSetViewModel
                {
                    Id = SelectedModelTemplate.Id,
                    ModelSet = modelSetResult,
                    Template = SelectedModelTemplate,
                });
            }
            else
            {
                modelTemplate.IsInstalled = true;
                modelTemplate.IsUpdateAvailable = false;
                if (modelTemplate.Group == ModelTemplateGroup.Custom)
                {
                    SelectedModelTemplate.Repository = addModelDialog.ModelFolder;
                }

                Settings.StableDiffusionModelSets.Add(new StableDiffusionModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSetResult,
                    Template = modelTemplate,
                });
            }

            NotifyPropertyChanged(nameof(SelectedModelTemplate));
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Uninstalls the stable diffusion model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UninstallStableDiffusionModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedStableDiffusionModel = Settings.StableDiffusionModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedStableDiffusionModel is null)
                return;

            await RemoveStableDiffusionModel(false);
            modelTemplate.IsInstalled = false;
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Updates the stable diffusion model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UpdateStableDiffusionModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedStableDiffusionModel = Settings.StableDiffusionModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedStableDiffusionModel is null)
                return;

            await UpdateStableDiffusionModel();
            SelectedModelTemplate.Name = SelectedStableDiffusionModel.Name;
            Settings.StableDiffusionModelSets.ForceNotifyCollectionChanged();
            await SaveAsync();
        }


        /// <summary>
        /// Removes the stable diffusion model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task RemoveStableDiffusionModel(ModelTemplateViewModel modelTemplate)
        {
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
                return; // Cant remove Templates

            SelectedStableDiffusionModel = Settings.StableDiffusionModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedStableDiffusionModel is not null)
                await RemoveStableDiffusionModel(true);

            Settings.Templates.Remove(x => x.Id == modelTemplate.Id);
            ModelTemplateRefresh();
            await SaveAsync();
        }

        #endregion

        #region ControlNet


        /// <summary>
        /// Installs the control net model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task InstallControlNetModel(ModelTemplateViewModel modelTemplate)
        {
            var addModelDialog = DialogService.GetDialog<AddControlNetModelDialog>();
            if (!await addModelDialog.ShowDialogAsync(modelTemplate))
                return; // User Canceled


            var modelSetResult = addModelDialog.ModelSetResult;
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
            {
                SelectedModelTemplate = AddTemplate(modelSetResult.Name, modelTemplate);
                SelectedModelTemplate.ControlNetTemplate = modelTemplate.ControlNetTemplate with
                {
                    PipelineTypes = addModelDialog.ControlNetTemplate.PipelineTypes
                };
                SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                Settings.ControlNetModelSets.Add(new ControlNetModelSetViewModel
                {
                    Id = SelectedModelTemplate.Id,
                    ModelSet = modelSetResult,
                    PipelineTypes = SelectedModelTemplate.ControlNetTemplate.PipelineTypes,
                    Template = SelectedModelTemplate
                });
            }
            else
            {
                modelTemplate.IsInstalled = true;
                modelTemplate.IsUpdateAvailable = false;
                if (modelTemplate.Group == ModelTemplateGroup.Custom)
                {
                    SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                    SelectedModelTemplate.ControlNetTemplate.PipelineTypes = addModelDialog.ControlNetTemplate.PipelineTypes;
                }

                Settings.ControlNetModelSets.Add(new ControlNetModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSetResult,
                    PipelineTypes = SelectedModelTemplate.ControlNetTemplate.PipelineTypes,
                    Template = modelTemplate
                });
            }

            NotifyPropertyChanged(nameof(SelectedModelTemplate));
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Uninstalls the control net model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UninstallControlNetModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedControlNetModel = Settings.ControlNetModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedControlNetModel is null)
                return;

            await RemoveControlNetModel(false);
            modelTemplate.IsInstalled = false;
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Updates the control net model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UpdateControlNetModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedControlNetModel = Settings.ControlNetModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedControlNetModel is null)
                return;

            await UpdateControlNetModel();
            SelectedModelTemplate.Name = SelectedControlNetModel.Name;
            Settings.ControlNetModelSets.ForceNotifyCollectionChanged();
            await SaveAsync();
        }


        /// <summary>
        /// Removes the control net model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task RemoveControlNetModel(ModelTemplateViewModel modelTemplate)
        {
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
                return; // Cant remove Templates

            SelectedControlNetModel = Settings.ControlNetModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedControlNetModel is not null)
                await RemoveControlNetModel(true);

            Settings.Templates.Remove(x => x.Id == modelTemplate.Id);
            ModelTemplateRefresh();
            await SaveAsync();
        }


        #endregion

        #region Upscale Model


        /// <summary>
        /// Installs the upscale model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task InstallUpscaleModel(ModelTemplateViewModel modelTemplate)
        {
            var addModelDialog = DialogService.GetDialog<AddUpscaleModelDialog>();
            if (!await addModelDialog.ShowDialogAsync(modelTemplate))
                return; // User Canceled
          

            var modelSetResult = addModelDialog.ModelSetResult;
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
            {
                SelectedModelTemplate = AddTemplate(modelSetResult.Name, modelTemplate);
                SelectedModelTemplate.UpscaleTemplate = modelTemplate.UpscaleTemplate with { };
                SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                Settings.UpscaleModelSets.Add(new UpscaleModelSetViewModel
                {
                    Id = SelectedModelTemplate.Id,
                    ModelSet = modelSetResult,
                    Template = SelectedModelTemplate
                });
            }
            else
            {
                modelTemplate.IsInstalled = true;
                modelTemplate.IsUpdateAvailable = false;
                if (modelTemplate.Group == ModelTemplateGroup.Custom)
                {
                    SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                }

                Settings.UpscaleModelSets.Add(new UpscaleModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSetResult,
                    Template = modelTemplate
                });
            }

            NotifyPropertyChanged(nameof(SelectedModelTemplate));
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Uninstalls the upscale model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UninstallUpscaleModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedUpscaleModel = Settings.UpscaleModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedUpscaleModel is null)
                return;

            await RemoveUpscaleModel(false);
            modelTemplate.IsInstalled = false;
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Updates the upscale model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UpdateUpscaleModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedUpscaleModel = Settings.UpscaleModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedUpscaleModel is null)
                return;

            await UpdateUpscaleModel();
            SelectedModelTemplate.Name = SelectedUpscaleModel.Name;
            Settings.UpscaleModelSets.ForceNotifyCollectionChanged();
            await SaveAsync();
        }


        /// <summary>
        /// Removes the upscale model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task RemoveUpscaleModel(ModelTemplateViewModel modelTemplate)
        {
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
                return; // Cant remove Templates

            SelectedUpscaleModel = Settings.UpscaleModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedUpscaleModel is not null)
                await RemoveUpscaleModel(true);

            Settings.Templates.Remove(x => x.Id == modelTemplate.Id);
            ModelTemplateRefresh();
            await SaveAsync();
        }


        #endregion

        #region FeatureExtractor Model


        /// <summary>
        /// Installs the feature extractor model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task InstallFeatureExtractorModel(ModelTemplateViewModel modelTemplate)
        {
            var addModelDialog = DialogService.GetDialog<AddFeatureExtractorModelDialog>();
            if (!await addModelDialog.ShowDialogAsync(modelTemplate))
                return; // User Canceled


            var modelSetResult = addModelDialog.ModelSetResult;
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
            {
                SelectedModelTemplate = AddTemplate(modelSetResult.Name, modelTemplate);
                SelectedModelTemplate.FeatureExtractorTemplate = modelTemplate.FeatureExtractorTemplate with { };
                SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                Settings.FeatureExtractorModelSets.Add(new FeatureExtractorModelSetViewModel
                {
                    Id = SelectedModelTemplate.Id,
                    ModelSet = modelSetResult,
                    IsControlNetSupported = modelTemplate.FeatureExtractorTemplate.IsControlNetSupported,
                    Template = SelectedModelTemplate
                });
            }
            else
            {
                modelTemplate.IsInstalled = true;
                modelTemplate.IsUpdateAvailable = false;
                if (modelTemplate.Group == ModelTemplateGroup.Custom)
                {
                    SelectedModelTemplate.Repository = Path.GetDirectoryName(addModelDialog.ModelFile);
                }

                Settings.FeatureExtractorModelSets.Add(new FeatureExtractorModelSetViewModel
                {
                    Id = modelTemplate.Id,
                    ModelSet = modelSetResult,
                    IsControlNetSupported = modelTemplate.FeatureExtractorTemplate.IsControlNetSupported,
                    Template = modelTemplate
                });
            }

            NotifyPropertyChanged(nameof(SelectedModelTemplate));
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Uninstalls the feature extractor model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UninstallFeatureExtractorModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedFeatureExtractorModel = Settings.FeatureExtractorModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedFeatureExtractorModel is null)
                return;

            await RemoveFeatureExtractorModel(false);
            modelTemplate.IsInstalled = false;
            ModelTemplateRefresh();
            await SaveAsync();
        }


        /// <summary>
        /// Updates the feature extractor model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task UpdateFeatureExtractorModel(ModelTemplateViewModel modelTemplate)
        {
            SelectedFeatureExtractorModel = Settings.FeatureExtractorModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedFeatureExtractorModel is null)
                return;

            await UpdateFeatureExtractorModel();
            SelectedModelTemplate.Name = SelectedFeatureExtractorModel.Name;
            Settings.FeatureExtractorModelSets.ForceNotifyCollectionChanged();
            await SaveAsync();
        }


        /// <summary>
        /// Removes the feature extractor model.
        /// </summary>
        /// <param name="modelTemplate">The model template.</param>
        private async Task RemoveFeatureExtractorModel(ModelTemplateViewModel modelTemplate)
        {
            if (modelTemplate.Group == ModelTemplateGroup.Fixed)
                return; // Cant remove Templates

            SelectedFeatureExtractorModel = Settings.FeatureExtractorModelSets.FirstOrDefault(x => x.Id == modelTemplate.Id);
            if (SelectedFeatureExtractorModel is not null)
                await RemoveFeatureExtractorModel(true);

            Settings.Templates.Remove(x => x.Id == modelTemplate.Id);
            ModelTemplateRefresh();
            await SaveAsync();
        }


        #endregion

    }

    public enum LayoutViewType
    {
        TileSmall = 0,
        TileLarge = 1,
        List = 2
    }


    public enum GroupHeader
    {
        [Description("Stable Diffusion Models")]
        StableDiffusion = 0,

        [Description("Upscale Models")]
        Upscaler = 1,

        [Description("ControlNet Models")]
        ControlNet = 2,

        [Description("Feature Extractor Models")]
        FeatureExtractor = 3,

        [Description("Model Install Templates")]
        InstallTemplates = 100
    }
}
