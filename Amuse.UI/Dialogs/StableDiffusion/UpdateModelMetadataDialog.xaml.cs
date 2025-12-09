using Amuse.UI.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amuse.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for UpdateModelMetadataDialog.xaml
    /// </summary>
    public partial class UpdateModelMetadataDialog : BaseDialog
    {
        private readonly AmuseSettings _settings;
        private ModelTemplateViewModel _modelTemplate;
        private string _validationError;
        private string _website;
        private string _author;
        private string _description;
        private string _iconImage;
        private string _repository;

        public UpdateModelMetadataDialog(AmuseSettings settings)
        {
            _settings = settings;
            PreviewImages = new ObservableCollection<string>();
            RepositoryFiles = new ObservableCollection<string>();
            InitializeComponent();
        }

        public ObservableCollection<string> PreviewImages { get; }
        public ObservableCollection<string> RepositoryFiles { get; }

        public string ValidationError
        {
            get { return _validationError; }
            set { _validationError = value; NotifyPropertyChanged(); }
        }

        public string Website
        {
            get { return _website; }
            set { _website = value; NotifyPropertyChanged(); }
        }

        public string Author
        {
            get { return _author; }
            set { _author = value; NotifyPropertyChanged(); }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; NotifyPropertyChanged(); }
        }

        public string IconImage
        {
            get { return _iconImage; }
            set { _iconImage = value; NotifyPropertyChanged(); }
        }

        public string Repository
        {
            get { return _repository; }
            set { _repository = value; NotifyPropertyChanged(); }
        }


        public Task<bool> ShowDialogAsync(ModelTemplateViewModel modelTemplate)
        {
            _modelTemplate = modelTemplate;

            Website = _modelTemplate.Website;
            Author = _modelTemplate.Author;
            Description = _modelTemplate.Description;
            IconImage = _modelTemplate.ImageIcon;
            Repository = _modelTemplate.Repository;

            for (int i = 0; i < 4; i++)
            {
                PreviewImages.Add(_modelTemplate.PreviewImages?.ElementAtOrDefault(i));
            }
            for (int i = 0; i < 12; i++)
            {
                RepositoryFiles.Add(_modelTemplate.RepositoryFiles?.ElementAtOrDefault(i));
            }
            return base.ShowDialogAsync();
        }


        protected override Task SaveAsync()
        {
            // validate links;
            _modelTemplate.Website = Website;
            _modelTemplate.Author = Author;
            _modelTemplate.Description = Description;
            _modelTemplate.Repository = Repository;
            _modelTemplate.RepositoryFiles = RepositoryFiles.Where(x => !string.IsNullOrEmpty(x)).ToList();
            _modelTemplate.ImageIcon = IconImage;
            _modelTemplate.PreviewImages = PreviewImages.ToList();

            var directory = Path.Combine(App.CacheDirectory, _modelTemplate.Id.ToString());
            Directory.CreateDirectory(directory);
            if (File.Exists(_modelTemplate.ImageIcon))
            {
                var destination = Path.Combine(directory, "Logo.png");
                if (!destination.Equals(_modelTemplate.ImageIcon))
                {
                    File.Copy(_modelTemplate.ImageIcon, destination, true);
                    _modelTemplate.ImageIcon = destination;
                }
            }

            for (int i = 0; i < PreviewImages.Count; i++)
            {
                if (string.IsNullOrEmpty(PreviewImages[i]))
                    continue;

                if (File.Exists(PreviewImages[i]))
                {
                    var destination = Path.Combine(directory, $"Preview{i + 1}.png");
                    if (!destination.Equals(PreviewImages[i]))
                    {
                        File.Copy(PreviewImages[i], destination, true);
                        _modelTemplate.PreviewImages[i] = destination;
                    }
                }
            }

            return base.SaveAsync();
        }
  
    }
}
