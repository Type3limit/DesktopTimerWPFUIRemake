using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopTimer.Helpers;
using DesktopTimer.models;
using DesktopTimer.Models.BackgroundWorkingModel.Definations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopTimer.Models.BackgroundWorkingModel
{
    public class WebRequestModel:ObservableObject
    {

        #region data
        private Dictionary<string, WebRequestBase> requestCollections = new Dictionary<string, WebRequestBase>();
        public Dictionary<string, WebRequestBase> RequestCollections
        {
            get => requestCollections;
            set => SetProperty(ref requestCollections, value);
        }


        private Dictionary<string, Type> requestTypes = new Dictionary<string, Type>()
        {
            { "WallHaven", typeof(WallHavenRequest) },
        };
        #endregion

        #region properties


        private List<string> useableRequest = new List<string>();
        public List<string> UseableRequest
        {
            get=>useableRequest;
            set=>SetProperty(ref useableRequest, value);
        }

        private string? selectedRequest = "";
        public string? SelectedRequest
        {
            get=>selectedRequest;
            set
            {
                SetProperty(ref selectedRequest, value);
                OnPropertyChanged("SelectedRequestInstance");
            }
        }

        /// <summary>
        /// selected webType
        /// </summary>
        public WebRequestBase? SelectedRequestInstance => SelectedRequest.IsNullOrEmpty() ? null : RequestCollections[SelectedRequest];

        #endregion


        #region constructor
        MainWorkModel? currentModelInstance = null;
        public WebRequestModel(MainWorkModel modelInstance)
        {
            currentModelInstance = modelInstance;
            ReadWebTypes();
            WriteWebTypes();
            Initialize();
        }

        #endregion


        #region command

        private ICommand? startRequestCommand = null;
        public ICommand? StartRequestCommand
        {
            get=>startRequestCommand??(startRequestCommand = new RelayCommand(() => 
            {

            }));
        }

        #endregion

        #region methods


        #region init
        private WebRequestBase? CreateInstance(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                return null;
            }

            var newExpression = Expression.New(constructor);
            var lambda = Expression.Lambda<Func<WebRequestBase>>(newExpression).Compile();
            return lambda();
        }

        private void ReadWebTypes()
        {
            var webConfig = FileMapper.WebSiteJson.ReadText();
            if(webConfig.IsNullOrEmpty())
                return;
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                Converters = { new TypeJsonConverter() }
            };
            var res = JsonSerializer.Deserialize<Dictionary<string, Type>>(webConfig,opt);
            if(res==null)
                return;
            requestTypes = res;
        }

        private void WriteWebTypes()
        {
            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
               Converters =  { new TypeJsonConverter() }
            };
            using(var stream = System.IO.File.Open(FileMapper.WebSiteJson,System.IO.FileMode.OpenOrCreate))
            {
                JsonSerializer.Serialize<Dictionary<string, Type>>(stream, requestTypes, opt);
            }
        }

        public void Initialize()
        { 
            foreach (var requestType in requestTypes)
            {
                var instance = CreateInstance(requestType.Value);
                if (instance != null)
                {
                    RequestCollections.Add(requestType.Key, instance);
                    UseableRequest.Add(requestType.Key);
                }
            }
            SelectedRequest = UseableRequest?.FirstOrDefault();
        }
        #endregion

        #region cache

        #endregion

        #endregion
    }
}
