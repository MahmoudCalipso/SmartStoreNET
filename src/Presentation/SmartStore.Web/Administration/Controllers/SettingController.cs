﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Settings;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.UI.Captcha;
using Telerik.Web.Mvc;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Themes;
using SmartStore.Services.Stores;
using SmartStore.Admin.Models.Stores;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class SettingController : AdminControllerBase
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IEncryptionService _encryptionService;
        private readonly IThemeRegistry _themeRegistry;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IFulltextService _fulltextService;
        private readonly IMaintenanceService _maintenanceService;
		private readonly IStoreService _storeService;
		private readonly IWorkContext _workContext;
		private readonly IGenericAttributeService _genericAttributeService;

        private CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private ShoppingCartSettings _shoppingCartSettings;
        private CustomerSettings _customerSettings;
        private AddressSettings _addressSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly SeoSettings _seoSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly PdfSettings _pdfSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
	    private readonly CommonSettings _commonSettings;

        //codehint: sm-add
        private readonly CompanyInformationSettings _companyInformationSettings;
        private readonly ContactDataSettings _contactDataSettings;
        private readonly BankConnectionSettings _bankConnectionSettings;
        private readonly SocialSettings _socialSettings;

		#endregion

		#region Constructors

        public SettingController(ISettingService settingService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IAddressService addressService, ITaxCategoryService taxCategoryService,
            ICurrencyService currencyService, IPictureService pictureService, 
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IOrderService orderService, IEncryptionService encryptionService,
            IThemeRegistry themeRegistry, ICustomerService customerService, 
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            IWebHelper webHelper, IFulltextService fulltextService,
			IMaintenanceService maintenanceService, IStoreService storeService,
			IWorkContext workContext, IGenericAttributeService genericAttributeService,
            CatalogSettings catalogSettings, 
            CurrencySettings currencySettings, 
            ShoppingCartSettings shoppingCartSettings, 
            CustomerSettings customerSettings, AddressSettings addressSettings,
            DateTimeSettings dateTimeSettings, StoreInformationSettings storeInformationSettings,
            SeoSettings seoSettings,SecuritySettings securitySettings, PdfSettings pdfSettings,
            LocalizationSettings localizationSettings, 
            CaptchaSettings captchaSettings, ExternalAuthenticationSettings externalAuthenticationSettings,
            CommonSettings commonSettings, CompanyInformationSettings companyInformationSettings,
            ContactDataSettings contactDataSettings, BankConnectionSettings bankConnectionSettings,
            SocialSettings socialSettings)
        {
            this._settingService = settingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._taxCategoryService = taxCategoryService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._orderService = orderService;
            this._encryptionService = encryptionService;
            this._themeRegistry = themeRegistry;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._webHelper = webHelper;
            this._fulltextService = fulltextService;
            this._maintenanceService = maintenanceService;
			this._storeService = storeService;
			this._workContext = workContext;
			this._genericAttributeService = genericAttributeService;

            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._customerSettings = customerSettings;
            this._addressSettings = addressSettings;
            this._dateTimeSettings = dateTimeSettings;
            this._storeInformationSettings = storeInformationSettings;
            this._seoSettings = seoSettings;
            this._securitySettings = securitySettings;
            this._pdfSettings = pdfSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;
            this._externalAuthenticationSettings = externalAuthenticationSettings;
            this._commonSettings = commonSettings;

            //codehint: sm-add
            this._companyInformationSettings = companyInformationSettings;
            this._contactDataSettings = contactDataSettings;
            this._bankConnectionSettings = bankConnectionSettings;
            this._socialSettings = socialSettings;
        }

		#endregion 

        #region Methods

		[NonAction]
		private int GetActiveStoreScopeConfiguration()
		{
			//ensure that we have 2 (or more) stores
			if (_storeService.GetAllStores().Count < 2)
				return 0;

			var storeId = _workContext.CurrentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
			var store = _storeService.GetStoreById(storeId);
			return store != null ? store.Id : 0;
		}

		[ChildActionOnly]
		public ActionResult StoreScopeConfiguration()
		{
			var allStores = _storeService.GetAllStores();
			if (allStores.Count < 2)
				return Content("");

			var model = new StoreScopeConfigurationModel();
			foreach (var s in allStores)
			{
				model.Stores.Add(new StoreModel()
				{
					Id = s.Id,
					Name = s.Name
				});
			}
			model.StoreId = GetActiveStoreScopeConfiguration();

			return PartialView(model);
		}

		public ActionResult ChangeStoreScopeConfiguration(int storeid, string returnUrl = "")
		{
			var store = _storeService.GetStoreById(storeid);
			if (store != null || storeid == 0)
			{
				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
			}
			//url referrer
			if (String.IsNullOrEmpty(returnUrl))
				returnUrl = _webHelper.GetUrlReferrer();
			//home page
			if (String.IsNullOrEmpty(returnUrl))
				returnUrl = Url.Action("Index", "Home");
			return Redirect(returnUrl);
		}

        public ActionResult Blog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
			var model = blogSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.Enabled = _settingService.SettingExists(storeScope, blogSettings, x => x.Enabled);
				model.PostsPageSize = _settingService.SettingExists(storeScope, blogSettings, x => x.PostsPageSize);
				model.AllowNotRegisteredUsersToLeaveComments = _settingService.SettingExists(storeScope, blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
				model.NotifyAboutNewBlogComments = _settingService.SettingExists(storeScope, blogSettings, x => x.NotifyAboutNewBlogComments);
				model.NumberOfTags = _settingService.SettingExists(storeScope, blogSettings, x => x.NumberOfTags);
				model.ShowHeaderRssUrl = _settingService.SettingExists(storeScope, blogSettings, x => x.ShowHeaderRssUrl);
			}

            return View(model);
        }
        [HttpPost]
        public ActionResult Blog(BlogSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
			blogSettings = model.ToEntity(blogSettings);

			_settingService.UpdateSetting(model.Enabled, storeScope, blogSettings, x => x.Enabled);
			_settingService.UpdateSetting(model.PostsPageSize, storeScope, blogSettings, x => x.PostsPageSize);
			_settingService.UpdateSetting(model.AllowNotRegisteredUsersToLeaveComments, storeScope, blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
			_settingService.UpdateSetting(model.NotifyAboutNewBlogComments, storeScope, blogSettings, x => x.NotifyAboutNewBlogComments);
			_settingService.UpdateSetting(model.NumberOfTags, storeScope, blogSettings, x => x.NumberOfTags);
			_settingService.UpdateSetting(model.ShowHeaderRssUrl, storeScope, blogSettings, x => x.ShowHeaderRssUrl);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }




        public ActionResult Forum()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
			var model = forumSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.ForumsEnabled = _settingService.SettingExists(storeScope, forumSettings, x => x.ForumsEnabled);
				model.RelativeDateTimeFormattingEnabled = _settingService.SettingExists(storeScope, forumSettings, x => x.RelativeDateTimeFormattingEnabled);
				model.ShowCustomersPostCount = _settingService.SettingExists(storeScope, forumSettings, x => x.ShowCustomersPostCount);
				model.AllowGuestsToCreatePosts = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowGuestsToCreatePosts);
				model.AllowGuestsToCreateTopics = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowGuestsToCreateTopics);
				model.AllowCustomersToEditPosts = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowCustomersToEditPosts);
				model.AllowCustomersToDeletePosts = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowCustomersToDeletePosts);
				model.AllowCustomersToManageSubscriptions = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowCustomersToManageSubscriptions);
				model.TopicsPageSize = _settingService.SettingExists(storeScope, forumSettings, x => x.TopicsPageSize);
				model.PostsPageSize = _settingService.SettingExists(storeScope, forumSettings, x => x.PostsPageSize);
				model.ForumEditor = _settingService.SettingExists(storeScope, forumSettings, x => x.ForumEditor);
				model.SignaturesEnabled = _settingService.SettingExists(storeScope, forumSettings, x => x.SignaturesEnabled);
				model.AllowPrivateMessages = _settingService.SettingExists(storeScope, forumSettings, x => x.AllowPrivateMessages);
				model.ShowAlertForPM = _settingService.SettingExists(storeScope, forumSettings, x => x.ShowAlertForPM);
				model.NotifyAboutPrivateMessages = _settingService.SettingExists(storeScope, forumSettings, x => x.NotifyAboutPrivateMessages);
				model.ActiveDiscussionsFeedEnabled = _settingService.SettingExists(storeScope, forumSettings, x => x.ActiveDiscussionsFeedEnabled);
				model.ActiveDiscussionsFeedCount = _settingService.SettingExists(storeScope, forumSettings, x => x.ActiveDiscussionsFeedCount);
				model.ForumFeedsEnabled = _settingService.SettingExists(storeScope, forumSettings, x => x.ForumFeedsEnabled);
				model.ForumFeedCount = _settingService.SettingExists(storeScope, forumSettings, x => x.ForumFeedCount);
				model.SearchResultsPageSize = _settingService.SettingExists(storeScope, forumSettings, x => x.SearchResultsPageSize);
			}
			model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList();
			
			return View(model);
        }
        [HttpPost]
        public ActionResult Forum(ForumSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
			forumSettings = model.ToEntity(forumSettings);

			_settingService.UpdateSetting(model.ForumsEnabled, storeScope, forumSettings, x => x.ForumsEnabled);
			_settingService.UpdateSetting(model.RelativeDateTimeFormattingEnabled, storeScope, forumSettings, x => x.RelativeDateTimeFormattingEnabled);
			_settingService.UpdateSetting(model.ShowCustomersPostCount, storeScope, forumSettings, x => x.ShowCustomersPostCount);
			_settingService.UpdateSetting(model.AllowGuestsToCreatePosts, storeScope, forumSettings, x => x.AllowGuestsToCreatePosts);
			_settingService.UpdateSetting(model.AllowGuestsToCreateTopics, storeScope, forumSettings, x => x.AllowGuestsToCreateTopics);
			_settingService.UpdateSetting(model.AllowCustomersToEditPosts, storeScope, forumSettings, x => x.AllowCustomersToEditPosts);
			_settingService.UpdateSetting(model.AllowCustomersToDeletePosts, storeScope, forumSettings, x => x.AllowCustomersToDeletePosts);
			_settingService.UpdateSetting(model.AllowCustomersToManageSubscriptions, storeScope, forumSettings, x => x.AllowCustomersToManageSubscriptions);
			_settingService.UpdateSetting(model.TopicsPageSize, storeScope, forumSettings, x => x.TopicsPageSize);
			_settingService.UpdateSetting(model.PostsPageSize, storeScope, forumSettings, x => x.PostsPageSize);
			_settingService.UpdateSetting(model.ForumEditor, storeScope, forumSettings, x => x.ForumEditor);
			_settingService.UpdateSetting(model.SignaturesEnabled, storeScope, forumSettings, x => x.SignaturesEnabled);
			_settingService.UpdateSetting(model.AllowPrivateMessages, storeScope, forumSettings, x => x.AllowPrivateMessages);
			_settingService.UpdateSetting(model.ShowAlertForPM, storeScope, forumSettings, x => x.ShowAlertForPM);
			_settingService.UpdateSetting(model.NotifyAboutPrivateMessages, storeScope, forumSettings, x => x.NotifyAboutPrivateMessages);
			_settingService.UpdateSetting(model.ActiveDiscussionsFeedEnabled, storeScope, forumSettings, x => x.ActiveDiscussionsFeedEnabled);
			_settingService.UpdateSetting(model.ActiveDiscussionsFeedCount, storeScope, forumSettings, x => x.ActiveDiscussionsFeedCount);
			_settingService.UpdateSetting(model.ForumFeedsEnabled, storeScope, forumSettings, x => x.ForumFeedsEnabled);
			_settingService.UpdateSetting(model.ForumFeedCount, storeScope, forumSettings, x => x.ForumFeedCount);
			_settingService.UpdateSetting(model.SearchResultsPageSize, storeScope, forumSettings, x => x.SearchResultsPageSize);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }




        public ActionResult News()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
			var model = newsSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.Enabled = _settingService.SettingExists(storeScope, newsSettings, x => x.Enabled);
				model.AllowNotRegisteredUsersToLeaveComments = _settingService.SettingExists(storeScope, newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
				model.NotifyAboutNewNewsComments = _settingService.SettingExists(storeScope, newsSettings, x => x.NotifyAboutNewNewsComments);
				model.ShowNewsOnMainPage = _settingService.SettingExists(storeScope, newsSettings, x => x.ShowNewsOnMainPage);
				model.MainPageNewsCount = _settingService.SettingExists(storeScope, newsSettings, x => x.MainPageNewsCount);
				model.NewsArchivePageSize = _settingService.SettingExists(storeScope, newsSettings, x => x.NewsArchivePageSize);
				model.ShowHeaderRssUrl = _settingService.SettingExists(storeScope, newsSettings, x => x.ShowHeaderRssUrl);
			}
            return View(model);
        }
        [HttpPost]
        public ActionResult News(NewsSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
			newsSettings = model.ToEntity(newsSettings);

			_settingService.UpdateSetting(model.Enabled, storeScope, newsSettings, x => x.Enabled);
			_settingService.UpdateSetting(model.AllowNotRegisteredUsersToLeaveComments, storeScope, newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments);
			_settingService.UpdateSetting(model.NotifyAboutNewNewsComments, storeScope, newsSettings, x => x.NotifyAboutNewNewsComments);
			_settingService.UpdateSetting(model.ShowNewsOnMainPage, storeScope, newsSettings, x => x.ShowNewsOnMainPage);
			_settingService.UpdateSetting(model.MainPageNewsCount, storeScope, newsSettings, x => x.MainPageNewsCount);
			_settingService.UpdateSetting(model.NewsArchivePageSize, storeScope, newsSettings, x => x.NewsArchivePageSize);
			_settingService.UpdateSetting(model.ShowHeaderRssUrl, storeScope, newsSettings, x => x.ShowHeaderRssUrl);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }




        public ActionResult Shipping()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
			var model = shippingSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.FreeShippingOverXEnabled = _settingService.SettingExists(storeScope, shippingSettings, x => x.FreeShippingOverXEnabled);
				model.FreeShippingOverXValue = _settingService.SettingExists(storeScope, shippingSettings, x => x.FreeShippingOverXValue);
				model.FreeShippingOverXIncludingTax = _settingService.SettingExists(storeScope, shippingSettings, x => x.FreeShippingOverXIncludingTax);
				model.EstimateShippingEnabled = _settingService.SettingExists(storeScope, shippingSettings, x => x.EstimateShippingEnabled);
				model.DisplayShipmentEventsToCustomers = _settingService.SettingExists(storeScope, shippingSettings, x => x.DisplayShipmentEventsToCustomers);
			}

			var originAddress = shippingSettings.ShippingOriginAddressId > 0
				? _addressService.GetAddressById(shippingSettings.ShippingOriginAddressId)
				: null;

			model.ShippingOriginAddress = new StoreDependingSetting<AddressModel>()
			{
				Value = (originAddress != null ? originAddress.ToModel() : new AddressModel()),
				OverrideForStore = (storeScope > 0 ? _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) : false)
			};

			// codehint: sm-delete
            // model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.ShippingOriginAddress.Value.AvailableCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) }
				);
			}

            var states = originAddress != null && originAddress.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(originAddress.Country.Id, true).ToList() : new List<StateProvince>();
			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.ShippingOriginAddress.Value.AvailableStates.Add(
						new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == originAddress.StateProvinceId) }
					);
				}
			}
			else
			{
				model.ShippingOriginAddress.Value.AvailableStates.Add(
					new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" }
				);
			}

            model.ShippingOriginAddress.Value.CountryEnabled = true;
            model.ShippingOriginAddress.Value.StateProvinceEnabled = true;
            model.ShippingOriginAddress.Value.ZipPostalCodeEnabled = true;
            model.ShippingOriginAddress.Value.ZipPostalCodeRequired = true;

            return View(model);
        }
        [HttpPost]
        public ActionResult Shipping(ShippingSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
			shippingSettings = model.ToEntity(shippingSettings);

			_settingService.UpdateSetting(model.FreeShippingOverXEnabled, storeScope, shippingSettings, x => x.FreeShippingOverXEnabled);
			_settingService.UpdateSetting(model.FreeShippingOverXValue, storeScope, shippingSettings, x => x.FreeShippingOverXValue);
			_settingService.UpdateSetting(model.FreeShippingOverXIncludingTax, storeScope, shippingSettings, x => x.FreeShippingOverXIncludingTax);
			_settingService.UpdateSetting(model.EstimateShippingEnabled, storeScope, shippingSettings, x => x.EstimateShippingEnabled);
			_settingService.UpdateSetting(model.DisplayShipmentEventsToCustomers, storeScope, shippingSettings, x => x.DisplayShipmentEventsToCustomers);

			if (model.ShippingOriginAddress.OverrideForStore || storeScope == 0)
			{
				//update address
				var addressId = _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ?
					shippingSettings.ShippingOriginAddressId : 0;
				var originAddress = _addressService.GetAddressById(addressId) ??
					new Core.Domain.Common.Address()
					{
						CreatedOnUtc = DateTime.UtcNow,
					};
				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.ShippingOriginAddress.Value.Id = addressId;
				originAddress = model.ShippingOriginAddress.Value.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				shippingSettings.ShippingOriginAddressId = originAddress.Id;

				_settingService.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
			}
			else
			{
				_settingService.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
			}

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }




        public ActionResult Tax()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = GetActiveStoreScopeConfiguration();
            var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
            var model = taxSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.PricesIncludeTax = _settingService.SettingExists(storeScope, taxSettings, x => x.PricesIncludeTax);
				model.AllowCustomersToSelectTaxDisplayType = _settingService.SettingExists(storeScope, taxSettings, x => x.AllowCustomersToSelectTaxDisplayType);
				model.TaxDisplayType = _settingService.SettingExists(storeScope, taxSettings, x => x.TaxDisplayType);
				model.DisplayTaxSuffix = _settingService.SettingExists(storeScope, taxSettings, x => x.DisplayTaxSuffix);
				model.DisplayTaxRates = _settingService.SettingExists(storeScope, taxSettings, x => x.DisplayTaxRates);
				model.HideZeroTax = _settingService.SettingExists(storeScope, taxSettings, x => x.HideZeroTax);
				model.HideTaxInOrderSummary = _settingService.SettingExists(storeScope, taxSettings, x => x.HideTaxInOrderSummary);
				model.TaxBasedOn = _settingService.SettingExists(storeScope, taxSettings, x => x.TaxBasedOn);
				model.ShippingIsTaxable = _settingService.SettingExists(storeScope, taxSettings, x => x.ShippingIsTaxable);
				model.ShippingPriceIncludesTax = _settingService.SettingExists(storeScope, taxSettings, x => x.ShippingPriceIncludesTax);
				model.ShippingTaxClassId = _settingService.SettingExists(storeScope, taxSettings, x => x.ShippingTaxClassId);
				model.PaymentMethodAdditionalFeeIsTaxable = _settingService.SettingExists(storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable);
				model.PaymentMethodAdditionalFeeIncludesTax = _settingService.SettingExists(storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax);
				model.PaymentMethodAdditionalFeeTaxClassId = _settingService.SettingExists(storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId);
				model.EuVatEnabled = _settingService.SettingExists(storeScope, taxSettings, x => x.EuVatEnabled);
				model.EuVatShopCountryId = _settingService.SettingExists(storeScope, taxSettings, x => x.EuVatShopCountryId);
				model.EuVatAllowVatExemption = _settingService.SettingExists(storeScope, taxSettings, x => x.EuVatAllowVatExemption);
				model.EuVatUseWebService = _settingService.SettingExists(storeScope, taxSettings, x => x.EuVatUseWebService);
				model.EuVatEmailAdminWhenNewVatSubmitted = _settingService.SettingExists(storeScope, taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted);
			}

            model.TaxBasedOnValues = taxSettings.TaxBasedOn.ToSelectList();
            model.TaxDisplayTypeValues = taxSettings.TaxDisplayType.ToSelectList();

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            // model.ShippingTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
			foreach (var tc in taxCategories)
			{
				model.ShippingTaxCategories.Add(
					new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId }
				);
			}
            // model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
			foreach (var tc in taxCategories)
			{
				model.PaymentMethodAdditionalFeeTaxCategories.Add(
					new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId }
				);
			}

            //EU VAT countries
            // model.EuVatShopCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" }); // codehint: sm-delete
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.EuVatShopCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId }
				);
			}

            //default tax address
            var defaultAddress = taxSettings.DefaultTaxAddressId > 0
                                     ? _addressService.GetAddressById(taxSettings.DefaultTaxAddressId)
                                     : null;

			model.DefaultTaxAddress = new StoreDependingSetting<AddressModel>()
			{
				Value = (defaultAddress != null ? defaultAddress.ToModel() : new AddressModel()),
				OverrideForStore = (storeScope > 0 ? _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) : false)
			};

            // model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" }); // codehint: sm-delete
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.DefaultTaxAddress.Value.AvailableCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) }
				);
			}

            var states = defaultAddress != null && defaultAddress.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(defaultAddress.Country.Id, true).ToList() : new List<StateProvince>();
			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.DefaultTaxAddress.Value.AvailableStates.Add(
						new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) }
					);
				}
			}
			else
			{
				model.DefaultTaxAddress.Value.AvailableStates.Add(
					new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" }
				);
			}
            model.DefaultTaxAddress.Value.CountryEnabled = true;
            model.DefaultTaxAddress.Value.StateProvinceEnabled = true;
            model.DefaultTaxAddress.Value.ZipPostalCodeEnabled = true;
            model.DefaultTaxAddress.Value.ZipPostalCodeRequired = true;

            return View(model);
        }
        [HttpPost]
        public ActionResult Tax(TaxSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
			taxSettings = model.ToEntity(taxSettings);

			_settingService.UpdateSetting(model.PricesIncludeTax, storeScope, taxSettings, x => x.PricesIncludeTax);
			//codehint: sm-edit
			//_settingService.UpdateSetting(model.AllowCustomersToSelectTaxDisplayType, storeScope, taxSettings, x => x.AllowCustomersToSelectTaxDisplayType);
			_settingService.UpdateSetting(model.TaxDisplayType, storeScope, taxSettings, x => x.TaxDisplayType);
			_settingService.UpdateSetting(model.DisplayTaxSuffix, storeScope, taxSettings, x => x.DisplayTaxSuffix);
			_settingService.UpdateSetting(model.DisplayTaxRates, storeScope, taxSettings, x => x.DisplayTaxRates);
			_settingService.UpdateSetting(model.HideZeroTax, storeScope, taxSettings, x => x.HideZeroTax);
			_settingService.UpdateSetting(model.HideTaxInOrderSummary, storeScope, taxSettings, x => x.HideTaxInOrderSummary);
			_settingService.UpdateSetting(model.TaxBasedOn, storeScope, taxSettings, x => x.TaxBasedOn);
			_settingService.UpdateSetting(model.ShippingIsTaxable, storeScope, taxSettings, x => x.ShippingIsTaxable);
			_settingService.UpdateSetting(model.ShippingPriceIncludesTax, storeScope, taxSettings, x => x.ShippingPriceIncludesTax);
			_settingService.UpdateSetting(model.ShippingTaxClassId, storeScope, taxSettings, x => x.ShippingTaxClassId);
			_settingService.UpdateSetting(model.PaymentMethodAdditionalFeeIsTaxable, storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeIsTaxable);
			_settingService.UpdateSetting(model.PaymentMethodAdditionalFeeIncludesTax, storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeIncludesTax);
			_settingService.UpdateSetting(model.PaymentMethodAdditionalFeeTaxClassId, storeScope, taxSettings, x => x.PaymentMethodAdditionalFeeTaxClassId);
			_settingService.UpdateSetting(model.EuVatEnabled, storeScope, taxSettings, x => x.EuVatEnabled);
			_settingService.UpdateSetting(model.EuVatShopCountryId, storeScope, taxSettings, x => x.EuVatShopCountryId);
			_settingService.UpdateSetting(model.EuVatAllowVatExemption, storeScope, taxSettings, x => x.EuVatAllowVatExemption);
			_settingService.UpdateSetting(model.EuVatUseWebService, storeScope, taxSettings, x => x.EuVatUseWebService);
			_settingService.UpdateSetting(model.EuVatEmailAdminWhenNewVatSubmitted, storeScope, taxSettings, x => x.EuVatEmailAdminWhenNewVatSubmitted);

			if (model.DefaultTaxAddress.OverrideForStore || storeScope == 0)
			{
				//update address
				var addressId = _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ?
					taxSettings.DefaultTaxAddressId : 0;
				var originAddress = _addressService.GetAddressById(addressId) ??
					new Core.Domain.Common.Address()
					{
						CreatedOnUtc = DateTime.UtcNow,
					};
				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.DefaultTaxAddress.Value.Id = addressId;
				originAddress = model.DefaultTaxAddress.Value.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				taxSettings.DefaultTaxAddressId = originAddress.Id;

				_settingService.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
			}
			else if (storeScope > 0)
			{
				_settingService.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);
			}

			//codehint: sm-add
			taxSettings.AllowCustomersToSelectTaxDisplayType = false;

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }




        public ActionResult Catalog(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _catalogSettings.ToModel();
            model.AvailableDefaultViewModes.Add(new SelectListItem { Value = "grid", Text = _localizationService.GetResource("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") });
            model.AvailableDefaultViewModes.Add(new SelectListItem { Value = "list", Text = _localizationService.GetResource("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") });

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult Catalog(CatalogSettingsModel model, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _catalogSettings = model.ToEntity(_catalogSettings);
            _settingService.SaveSetting(_catalogSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Catalog", new { selectedTab = selectedTab });
        }



        public ActionResult RewardPoints()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();


			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
			var model = rewardPointsSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.Enabled = _settingService.SettingExists(storeScope, rewardPointsSettings, x => x.Enabled);
				model.ExchangeRate = _settingService.SettingExists(storeScope, rewardPointsSettings, x => x.ExchangeRate);
				model.PointsForRegistration = _settingService.SettingExists(storeScope, rewardPointsSettings, x => x.PointsForRegistration);

				model.PointsForPurchases_Amount = new StoreDependingSetting<decimal>(
					_settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope) ||
					_settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope)
				);					

				model.PointsForPurchases_Awarded = _settingService.SettingExists(storeScope, rewardPointsSettings, x => (int)x.PointsForPurchases_Awarded);
				model.PointsForPurchases_Canceled = _settingService.SettingExists(storeScope, rewardPointsSettings, x => (int)x.PointsForPurchases_Canceled);
			}
			model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
			
			return View(model);
        }
        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			if (ModelState.IsValid)
			{
				//load settings for a chosen store scope
				var storeScope = GetActiveStoreScopeConfiguration();
				var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
				rewardPointsSettings = model.ToEntity(rewardPointsSettings);

				_settingService.UpdateSetting(model.Enabled, storeScope, rewardPointsSettings, x => x.Enabled);
				_settingService.UpdateSetting(model.ExchangeRate, storeScope, rewardPointsSettings, x => x.ExchangeRate);
				_settingService.UpdateSetting(model.PointsForRegistration, storeScope, rewardPointsSettings, x => x.PointsForRegistration);

				_settingService.UpdateSetting(model.PointsForPurchases_Amount, storeScope, rewardPointsSettings, x => x.PointsForPurchases_Amount);
				_settingService.UpdateSetting(model.PointsForPurchases_Amount, storeScope, rewardPointsSettings, x => x.PointsForPurchases_Points);

				_settingService.UpdateSetting(model.PointsForPurchases_Awarded, storeScope, rewardPointsSettings, x => (int)x.PointsForPurchases_Awarded);
				_settingService.UpdateSetting(model.PointsForPurchases_Canceled, storeScope, rewardPointsSettings, x => (int)x.PointsForPurchases_Canceled);

				//now clear settings cache
				_settingService.ClearCache();

				//activity log
				_customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

				SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
			}
			else
			{
				//If we got this far, something failed, redisplay form
				foreach (var modelState in ModelState.Values)
					foreach (var error in modelState.Errors)
						ErrorNotification(error.ErrorMessage);
			}
			return RedirectToAction("RewardPoints");
        }




        public ActionResult Order(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
			var model = orderSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.IsReOrderAllowed = _settingService.SettingExists(storeScope, orderSettings, x => x.IsReOrderAllowed);
				model.MinOrderSubtotalAmount = _settingService.SettingExists(storeScope, orderSettings, x => x.MinOrderSubtotalAmount);
				model.MinOrderTotalAmount = _settingService.SettingExists(storeScope, orderSettings, x => x.MinOrderTotalAmount);
				model.AnonymousCheckoutAllowed = _settingService.SettingExists(storeScope, orderSettings, x => x.AnonymousCheckoutAllowed);
				model.TermsOfServiceEnabled = _settingService.SettingExists(storeScope, orderSettings, x => x.TermsOfServiceEnabled);
				model.OnePageCheckoutEnabled = _settingService.SettingExists(storeScope, orderSettings, x => x.OnePageCheckoutEnabled);
				model.ReturnRequestsEnabled = _settingService.SettingExists(storeScope, orderSettings, x => x.ReturnRequestsEnabled);
				model.NumberOfDaysReturnRequestAvailable = _settingService.SettingExists(storeScope, orderSettings, x => x.NumberOfDaysReturnRequestAvailable);
			}

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            //gift card activation/deactivation
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            //model.GiftCards_Activated_OrderStatuses.Insert(0, new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            //model.GiftCards_Deactivated_OrderStatuses.Insert(0, new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete


            //parse return request actions
			for (int i = 0; i < orderSettings.ReturnRequestActions.Count; i++)
            {
				model.ReturnRequestActionsParsed += orderSettings.ReturnRequestActions[i];
				if (i != orderSettings.ReturnRequestActions.Count - 1)
                    model.ReturnRequestActionsParsed += ",";
            }
            //parse return request reasons
			for (int i = 0; i < orderSettings.ReturnRequestReasons.Count; i++)
            {
				model.ReturnRequestReasonsParsed += orderSettings.ReturnRequestReasons[i];
				if (i != orderSettings.ReturnRequestReasons.Count - 1)
                    model.ReturnRequestReasonsParsed += ",";
            }

            //order ident
            model.OrderIdent = _maintenanceService.GetTableIdent<Order>();

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult Order(OrderSettingsModel model, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
				//load settings for a chosen store scope
				var storeScope = GetActiveStoreScopeConfiguration();
				var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
				orderSettings = model.ToEntity(orderSettings);

				_settingService.UpdateSetting(model.IsReOrderAllowed, storeScope, orderSettings, x => x.IsReOrderAllowed);
				_settingService.UpdateSetting(model.MinOrderSubtotalAmount, storeScope, orderSettings, x => x.MinOrderSubtotalAmount);
				_settingService.UpdateSetting(model.MinOrderTotalAmount, storeScope, orderSettings, x => x.MinOrderTotalAmount);
				_settingService.UpdateSetting(model.AnonymousCheckoutAllowed, storeScope, orderSettings, x => x.AnonymousCheckoutAllowed);
				_settingService.UpdateSetting(model.TermsOfServiceEnabled, storeScope, orderSettings, x => x.TermsOfServiceEnabled);
				_settingService.UpdateSetting(model.OnePageCheckoutEnabled, storeScope, orderSettings, x => x.OnePageCheckoutEnabled);
				_settingService.UpdateSetting(model.ReturnRequestsEnabled, storeScope, orderSettings, x => x.ReturnRequestsEnabled);
				_settingService.UpdateSetting(model.NumberOfDaysReturnRequestAvailable, storeScope, orderSettings, x => x.NumberOfDaysReturnRequestAvailable);

                model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

				//parse return request actions
				orderSettings.ReturnRequestActions.Clear();
				foreach (var returnAction in model.ReturnRequestActionsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					orderSettings.ReturnRequestActions.Add(returnAction);
				_settingService.SaveSetting(orderSettings, x => x.ReturnRequestActions, storeScope, false);
				
				//parse return request reasons
				orderSettings.ReturnRequestReasons.Clear();
				foreach (var returnReason in model.ReturnRequestReasonsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					orderSettings.ReturnRequestReasons.Add(returnReason);
				_settingService.SaveSetting(orderSettings, x => x.ReturnRequestReasons, storeScope, false);

				_settingService.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, 0, false);
				_settingService.SaveSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId, 0, false);

				//now clear settings cache
				_settingService.ClearCache();

                //order ident
                if (model.OrderIdent.HasValue)
                {
                    try
                    {
                        _maintenanceService.SetTableIdent<Order>(model.OrderIdent.Value);
                    }
                    catch (Exception exc)
                    {
                        ErrorNotification(exc.Message);
                    }
                }

                //activity log
                _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            }
            else
            {
				//If we got this far, something failed, redisplay form
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
                        ErrorNotification(error.ErrorMessage);
            }
            return RedirectToAction("Order", new { selectedTab = selectedTab });
        }




        public ActionResult ShoppingCart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = _shoppingCartSettings.ToModel();
            return View(model);
        }
        [HttpPost]
        public ActionResult ShoppingCart(ShoppingCartSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _shoppingCartSettings = model.ToEntity(_shoppingCartSettings);
            _settingService.SaveSetting(_shoppingCartSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }




        public ActionResult Media()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
			var model = mediaSettings.ToModel();
			model.ActiveStoreScopeConfiguration = storeScope;
			if (storeScope > 0)
			{
				model.AvatarPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.AvatarPictureSize);
				model.ProductThumbPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.ProductThumbPictureSize);
				model.ProductDetailsPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.ProductDetailsPictureSize);
				model.ProductThumbPictureSizeOnProductDetailsPage = _settingService.SettingExists(storeScope, mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage);
				model.ProductVariantPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.ProductVariantPictureSize);
				model.CategoryThumbPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.CategoryThumbPictureSize);
				model.ManufacturerThumbPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.ManufacturerThumbPictureSize);
				model.CartThumbPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.CartThumbPictureSize);
				model.MiniCartThumbPictureSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.MiniCartThumbPictureSize);
				model.MaximumImageSize = _settingService.SettingExists(storeScope, mediaSettings, x => x.MaximumImageSize);
				model.DefaultPictureZoomEnabled = _settingService.SettingExists(storeScope, mediaSettings, x => x.DefaultPictureZoomEnabled);
				model.PictureZoomType = _settingService.SettingExists(storeScope, mediaSettings, x => x.PictureZoomType);
			}
            model.PicturesStoredIntoDatabase = _pictureService.StoreInDb;

            var resKey = "Admin.Configuration.Settings.Media.PictureZoomType.";
            
            model.AvailablePictureZoomTypes.Add(new SelectListItem { 
                Text = _localizationService.GetResource(resKey + "Window"), 
                Value = "window", 
                Selected = model.PictureZoomType.Equals("window") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem {
                Text = _localizationService.GetResource(resKey + "Inner"),
                Value = "inner", 
                Selected = model.PictureZoomType.Equals("inner") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem {
                Text = _localizationService.GetResource(resKey + "Lens"),
                Value = "lens", 
                Selected = model.PictureZoomType.Equals("lens") 
            });

            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Media(MediaSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = GetActiveStoreScopeConfiguration();
			var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
			mediaSettings = model.ToEntity(mediaSettings);

			_settingService.UpdateSetting(model.AvatarPictureSize, storeScope, mediaSettings, x => x.AvatarPictureSize);
			_settingService.UpdateSetting(model.ProductThumbPictureSize, storeScope, mediaSettings, x => x.ProductThumbPictureSize);
			_settingService.UpdateSetting(model.ProductDetailsPictureSize, storeScope, mediaSettings, x => x.ProductDetailsPictureSize);
			_settingService.UpdateSetting(model.ProductThumbPictureSizeOnProductDetailsPage, storeScope, mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage);
			_settingService.UpdateSetting(model.ProductVariantPictureSize, storeScope, mediaSettings, x => x.ProductVariantPictureSize);
			_settingService.UpdateSetting(model.CategoryThumbPictureSize, storeScope, mediaSettings, x => x.CategoryThumbPictureSize);
			_settingService.UpdateSetting(model.ManufacturerThumbPictureSize, storeScope, mediaSettings, x => x.ManufacturerThumbPictureSize);
			_settingService.UpdateSetting(model.CartThumbPictureSize, storeScope, mediaSettings, x => x.CartThumbPictureSize);
			_settingService.UpdateSetting(model.MiniCartThumbPictureSize, storeScope, mediaSettings, x => x.MiniCartThumbPictureSize);
			_settingService.UpdateSetting(model.MaximumImageSize, storeScope, mediaSettings, x => x.MaximumImageSize);
			_settingService.UpdateSetting(model.DefaultPictureZoomEnabled, storeScope, mediaSettings, x => x.DefaultPictureZoomEnabled);
			_settingService.UpdateSetting(model.PictureZoomType, storeScope, mediaSettings, x => x.PictureZoomType);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }
        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public ActionResult ChangePictureStorage()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _pictureService.StoreInDb = !_pictureService.StoreInDb;

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }



        public ActionResult CustomerUser(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //merge settings
            var model = new CustomerUserSettingsModel();
            model.CustomerSettings = _customerSettings.ToModel();
            model.AddressSettings = _addressSettings.ToModel();

            model.DateTimeSettings.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;
            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem()
                    {
                        Text = timeZone.DisplayName,
                        Value = timeZone.Id,
                        Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.InvariantCultureIgnoreCase)
                    });
            }

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = _externalAuthenticationSettings.AutoRegisterEnabled;

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult CustomerUser(CustomerUserSettingsModel model, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _customerSettings = model.CustomerSettings.ToEntity(_customerSettings);
            _settingService.SaveSetting(_customerSettings);

            _addressSettings = model.AddressSettings.ToEntity(_addressSettings);
            _settingService.SaveSetting(_addressSettings);

            _dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            _dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            _settingService.SaveSetting(_dateTimeSettings);

            _externalAuthenticationSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;
            _settingService.SaveSetting(_externalAuthenticationSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("CustomerUser", new { selectedTab = selectedTab });
        }






        public ActionResult GeneralCommon(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            //store information
            var model = new GeneralCommonSettingsModel();
            model.StoreInformationSettings.LogoPictureId = _storeInformationSettings.LogoPictureId;
            model.StoreInformationSettings.StoreClosed = _storeInformationSettings.StoreClosed;
            model.StoreInformationSettings.StoreClosedAllowForAdmins = _storeInformationSettings.StoreClosedAllowForAdmins;

            //seo settings
            model.SeoSettings.PageTitleSeparator = _seoSettings.PageTitleSeparator;
            model.SeoSettings.DefaultTitle = _seoSettings.DefaultTitle;
            model.SeoSettings.DefaultMetaKeywords = _seoSettings.DefaultMetaKeywords;
            model.SeoSettings.DefaultMetaDescription = _seoSettings.DefaultMetaDescription;
            model.SeoSettings.ConvertNonWesternChars = _seoSettings.ConvertNonWesternChars;
            model.SeoSettings.CanonicalUrlsEnabled = _seoSettings.CanonicalUrlsEnabled;
            model.SeoSettings.PageTitleSeoAdjustmentValues = _seoSettings.PageTitleSeoAdjustment.ToSelectList();
            
            //security settings
            model.SecuritySettings.EncryptionKey = _securitySettings.EncryptionKey;
            if (_securitySettings.AdminAreaAllowedIpAddresses!=null)
                for (int i=0;i<_securitySettings.AdminAreaAllowedIpAddresses.Count; i++)
                {
                    model.SecuritySettings.AdminAreaAllowedIpAddresses += _securitySettings.AdminAreaAllowedIpAddresses[i];
                    if (i != _securitySettings.AdminAreaAllowedIpAddresses.Count - 1)
                        model.SecuritySettings.AdminAreaAllowedIpAddresses += ",";
                }
            model.SecuritySettings.HideAdminMenuItemsBasedOnPermissions = _securitySettings.HideAdminMenuItemsBasedOnPermissions;
            model.SecuritySettings.CaptchaEnabled = _captchaSettings.Enabled;
            model.SecuritySettings.CaptchaShowOnLoginPage = _captchaSettings.ShowOnLoginPage;
            model.SecuritySettings.CaptchaShowOnRegistrationPage = _captchaSettings.ShowOnRegistrationPage;
            model.SecuritySettings.CaptchaShowOnContactUsPage = _captchaSettings.ShowOnContactUsPage;
            model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage = _captchaSettings.ShowOnEmailWishlistToFriendPage;
            model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage = _captchaSettings.ShowOnEmailProductToFriendPage;
            model.SecuritySettings.CaptchaShowOnAskQuestionPage = _captchaSettings.ShowOnAskQuestionPage;
            model.SecuritySettings.CaptchaShowOnBlogCommentPage = _captchaSettings.ShowOnBlogCommentPage;
            model.SecuritySettings.CaptchaShowOnNewsCommentPage = _captchaSettings.ShowOnNewsCommentPage;
            model.SecuritySettings.CaptchaShowOnProductReviewPage = _captchaSettings.ShowOnProductReviewPage;
            model.SecuritySettings.ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey;
            model.SecuritySettings.ReCaptchaPrivateKey = _captchaSettings.ReCaptchaPrivateKey;

            //PDF settings
            model.PdfSettings.Enabled = _pdfSettings.Enabled;
            model.PdfSettings.LetterPageSizeEnabled = _pdfSettings.LetterPageSizeEnabled;
            model.PdfSettings.LogoPictureId = _pdfSettings.LogoPictureId;

            //localization
            model.LocalizationSettings.UseImagesForLanguageSelection = _localizationSettings.UseImagesForLanguageSelection;
            model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled = _localizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
            model.LocalizationSettings.LoadAllLocaleRecordsOnStartup = _localizationSettings.LoadAllLocaleRecordsOnStartup;

            //full-text support
            model.FullTextSettings.Supported = _fulltextService.IsFullTextSupported();
            model.FullTextSettings.Enabled = _commonSettings.UseFullTextSearch;
            model.FullTextSettings.SearchModeValues = _commonSettings.FullTextMode.ToSelectList();

            //codehint: sm-add begin
            //company information
            model.CompanyInformationSettings.CompanyName = _companyInformationSettings.CompanyName;
            model.CompanyInformationSettings.Salutation = _companyInformationSettings.Salutation;
            model.CompanyInformationSettings.Title = _companyInformationSettings.Title;
            model.CompanyInformationSettings.Firstname = _companyInformationSettings.Firstname;
            model.CompanyInformationSettings.Lastname = _companyInformationSettings.Lastname;
            model.CompanyInformationSettings.CompanyManagementDescription = _companyInformationSettings.CompanyManagementDescription;
            model.CompanyInformationSettings.CompanyManagement = _companyInformationSettings.CompanyManagement;
            model.CompanyInformationSettings.Street = _companyInformationSettings.Street;
            model.CompanyInformationSettings.Street2 = _companyInformationSettings.Street2;
            model.CompanyInformationSettings.ZipCode = _companyInformationSettings.ZipCode;
            model.CompanyInformationSettings.City = _companyInformationSettings.City;
            model.CompanyInformationSettings.CountryId = _companyInformationSettings.CountryId;
            model.CompanyInformationSettings.Region = _companyInformationSettings.Region;
            model.CompanyInformationSettings.VatId = _companyInformationSettings.VatId;
            model.CompanyInformationSettings.CommercialRegister = _companyInformationSettings.CommercialRegister;
            model.CompanyInformationSettings.TaxNumber = _companyInformationSettings.TaxNumber;

            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.CompanyInformationSettings.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CompanyInformationSettings.CountryId) });
            }

            model.CompanyInformationSettings.Salutations.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.Salutation.Mr"), Value = _localizationService.GetResource("Admin.Address.Salutation.Mr") });
            model.CompanyInformationSettings.Salutations.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.Salutation.Mrs"), Value = _localizationService.GetResource("Admin.Address.Salutation.Mrs") });

            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Manager"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Manager") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shopkeeper"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shopkeeper") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Procurator"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Procurator") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shareholder"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shareholder") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.AuthorizedPartner"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.AuthorizedPartner") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Director"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Director") });
            model.CompanyInformationSettings.ManagementDescriptions.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.ManagingPartner"), Value = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.ManagingPartner") });

            //contact data
            model.ContactDataSettings.CompanyTelephoneNumber = _contactDataSettings.CompanyTelephoneNumber;
            model.ContactDataSettings.HotlineTelephoneNumber = _contactDataSettings.HotlineTelephoneNumber;
            model.ContactDataSettings.MobileTelephoneNumber = _contactDataSettings.MobileTelephoneNumber;
            model.ContactDataSettings.CompanyFaxNumber = _contactDataSettings.CompanyFaxNumber;
            model.ContactDataSettings.CompanyEmailAddress = _contactDataSettings.CompanyEmailAddress;
            model.ContactDataSettings.WebmasterEmailAddress = _contactDataSettings.WebmasterEmailAddress;
            model.ContactDataSettings.SupportEmailAddress = _contactDataSettings.SupportEmailAddress;
            model.ContactDataSettings.ContactEmailAddress = _contactDataSettings.ContactEmailAddress;

            //bank connection
            model.BankConnectionSettings.Bankname = _bankConnectionSettings.Bankname;
            model.BankConnectionSettings.Bankcode = _bankConnectionSettings.Bankcode;
            model.BankConnectionSettings.AccountNumber = _bankConnectionSettings.AccountNumber;
            model.BankConnectionSettings.AccountHolder = _bankConnectionSettings.AccountHolder;
            model.BankConnectionSettings.Iban = _bankConnectionSettings.Iban;
            model.BankConnectionSettings.Bic = _bankConnectionSettings.Bic;

            //social
            model.SocialSettings.ShowSocialLinksInFooter = _socialSettings.ShowSocialLinksInFooter;
            model.SocialSettings.FacebookLink = _socialSettings.FacebookLink;
            model.SocialSettings.GooglePlusLink = _socialSettings.GooglePlusLink;
            model.SocialSettings.TwitterLink = _socialSettings.TwitterLink;
            model.SocialSettings.PinterestLink = _socialSettings.PinterestLink;

            //codehint: sm-add end

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult GeneralCommon(GeneralCommonSettingsModel model, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            
            //store information
            _storeInformationSettings.LogoPictureId = model.StoreInformationSettings.LogoPictureId;
            _storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
            _storeInformationSettings.StoreClosedAllowForAdmins = model.StoreInformationSettings.StoreClosedAllowForAdmins;

            _settingService.SaveSetting(_storeInformationSettings);



            //seo settings
            _seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
            _seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
            _seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
            _seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
            _seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
            _seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
            _seoSettings.PageTitleSeoAdjustment = model.SeoSettings.PageTitleSeoAdjustment;
            _settingService.SaveSetting(_seoSettings);



            //security settings
            if (_securitySettings.AdminAreaAllowedIpAddresses == null)
                _securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
            _securitySettings.AdminAreaAllowedIpAddresses.Clear();
            if (!String.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                foreach (string s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    if (!String.IsNullOrWhiteSpace(s))
                        _securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
            _securitySettings.HideAdminMenuItemsBasedOnPermissions = model.SecuritySettings.HideAdminMenuItemsBasedOnPermissions;
            _settingService.SaveSetting(_securitySettings);
            _captchaSettings.Enabled = model.SecuritySettings.CaptchaEnabled;
            _captchaSettings.ShowOnLoginPage = model.SecuritySettings.CaptchaShowOnLoginPage;
            _captchaSettings.ShowOnRegistrationPage = model.SecuritySettings.CaptchaShowOnRegistrationPage;
            _captchaSettings.ShowOnContactUsPage = model.SecuritySettings.CaptchaShowOnContactUsPage;
            _captchaSettings.ShowOnEmailWishlistToFriendPage = model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage;
            _captchaSettings.ShowOnEmailProductToFriendPage = model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage;
            _captchaSettings.ShowOnAskQuestionPage = model.SecuritySettings.CaptchaShowOnAskQuestionPage;
            _captchaSettings.ShowOnBlogCommentPage = model.SecuritySettings.CaptchaShowOnBlogCommentPage;
            _captchaSettings.ShowOnNewsCommentPage = model.SecuritySettings.CaptchaShowOnNewsCommentPage;
            _captchaSettings.ShowOnProductReviewPage = model.SecuritySettings.CaptchaShowOnProductReviewPage;
            _captchaSettings.ReCaptchaPublicKey = model.SecuritySettings.ReCaptchaPublicKey;
            _captchaSettings.ReCaptchaPrivateKey = model.SecuritySettings.ReCaptchaPrivateKey;
            _settingService.SaveSetting(_captchaSettings);
            if (_captchaSettings.Enabled &&
                (String.IsNullOrWhiteSpace(_captchaSettings.ReCaptchaPublicKey) || String.IsNullOrWhiteSpace(_captchaSettings.ReCaptchaPrivateKey)))
            {
                //captcha is enabled but the keys are not entered
                ErrorNotification("Captcha is enabled but the appropriate keys are not entered");
            }

            //PDF settings
            _pdfSettings.Enabled = model.PdfSettings.Enabled;
            _pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
            _pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
            _settingService.SaveSetting(_pdfSettings);


            //localization settings
            _localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                _localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
                //clear cached values of routes
                System.Web.Routing.RouteTable.Routes.ClearSeoFriendlyUrlsCachedValueForRoutes();
            }
            _localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
            _settingService.SaveSetting(_localizationSettings);

            //full-text
            _commonSettings.FullTextMode = model.FullTextSettings.SearchMode;
            _settingService.SaveSetting(_commonSettings);

            //codehint: sm-add begin
            //company information
            _companyInformationSettings.CompanyName = model.CompanyInformationSettings.CompanyName;
            _companyInformationSettings.Salutation = model.CompanyInformationSettings.Salutation;
            _companyInformationSettings.Title = model.CompanyInformationSettings.Title;
            _companyInformationSettings.Firstname = model.CompanyInformationSettings.Firstname;
            _companyInformationSettings.Lastname = model.CompanyInformationSettings.Lastname;
            _companyInformationSettings.CompanyManagementDescription = model.CompanyInformationSettings.CompanyManagementDescription;
            _companyInformationSettings.CompanyManagement = model.CompanyInformationSettings.CompanyManagement;
            _companyInformationSettings.Street = model.CompanyInformationSettings.Street;
            _companyInformationSettings.Street2 = model.CompanyInformationSettings.Street2;
            _companyInformationSettings.ZipCode= model.CompanyInformationSettings.ZipCode;
            _companyInformationSettings.City = model.CompanyInformationSettings.City;
            _companyInformationSettings.CountryId = model.CompanyInformationSettings.CountryId;
            _companyInformationSettings.Region = model.CompanyInformationSettings.Region;
            if (model.CompanyInformationSettings.CountryId != 0)
            {
                _companyInformationSettings.CountryName = _countryService.GetCountryById(model.CompanyInformationSettings.CountryId).Name;
            }
            _companyInformationSettings.VatId = model.CompanyInformationSettings.VatId;
            _companyInformationSettings.CommercialRegister = model.CompanyInformationSettings.CommercialRegister;
            _companyInformationSettings.TaxNumber = model.CompanyInformationSettings.TaxNumber;
            _settingService.SaveSetting(_companyInformationSettings);

            //contact data
            _contactDataSettings.CompanyTelephoneNumber = model.ContactDataSettings.CompanyTelephoneNumber;
            _contactDataSettings.HotlineTelephoneNumber = model.ContactDataSettings.HotlineTelephoneNumber;
            _contactDataSettings.MobileTelephoneNumber = model.ContactDataSettings.MobileTelephoneNumber;
            _contactDataSettings.CompanyFaxNumber = model.ContactDataSettings.CompanyFaxNumber;
            _contactDataSettings.CompanyEmailAddress = model.ContactDataSettings.CompanyEmailAddress;
            _contactDataSettings.WebmasterEmailAddress = model.ContactDataSettings.WebmasterEmailAddress;
            _contactDataSettings.SupportEmailAddress = model.ContactDataSettings.SupportEmailAddress;
            _contactDataSettings.ContactEmailAddress = model.ContactDataSettings.ContactEmailAddress;

            if (ModelState.IsValid)
            {
                _settingService.SaveSetting(_contactDataSettings);
            }
            else 
            {
                return View(model);    
            }


            //bank connection
            _bankConnectionSettings.Bankname = model.BankConnectionSettings.Bankname;
            _bankConnectionSettings.Bankcode = model.BankConnectionSettings.Bankcode;
            _bankConnectionSettings.AccountNumber = model.BankConnectionSettings.AccountNumber;
            _bankConnectionSettings.AccountHolder = model.BankConnectionSettings.AccountHolder;
            _bankConnectionSettings.Iban = model.BankConnectionSettings.Iban;
            _bankConnectionSettings.Bic = model.BankConnectionSettings.Bic;
            _settingService.SaveSetting(_bankConnectionSettings);

            //social settings
            _socialSettings.ShowSocialLinksInFooter = model.SocialSettings.ShowSocialLinksInFooter;
            _socialSettings.FacebookLink = model.SocialSettings.FacebookLink;
            _socialSettings.GooglePlusLink = model.SocialSettings.GooglePlusLink;
            _socialSettings.TwitterLink = model.SocialSettings.TwitterLink;
            _socialSettings.PinterestLink = model.SocialSettings.PinterestLink;

            _settingService.SaveSetting(_socialSettings);

            //codehint: sm-add end

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("GeneralCommon", new { selectedTab = selectedTab });
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                string oldEncryptionPrivateKey = _securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                //update encrypted order info
                var orders = _orderService.LoadAllOrders();
                foreach (var order in orders)
                {
                    // new credit card encryption
                    string decryptedCardType = _encryptionService.DecryptText(order.CardType, oldEncryptionPrivateKey);
                    string decryptedCardName = _encryptionService.DecryptText(order.CardName, oldEncryptionPrivateKey);
                    string decryptedCardNumber = _encryptionService.DecryptText(order.CardNumber, oldEncryptionPrivateKey);
                    string decryptedMaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber, oldEncryptionPrivateKey);
                    string decryptedCardCvv2 = _encryptionService.DecryptText(order.CardCvv2, oldEncryptionPrivateKey);
                    string decryptedCardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth, oldEncryptionPrivateKey);
                    string decryptedCardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear, oldEncryptionPrivateKey);

                    string encryptedCardType = _encryptionService.EncryptText(decryptedCardType, newEncryptionPrivateKey);
                    string encryptedCardName = _encryptionService.EncryptText(decryptedCardName, newEncryptionPrivateKey);
                    string encryptedCardNumber = _encryptionService.EncryptText(decryptedCardNumber, newEncryptionPrivateKey);
                    string encryptedMaskedCreditCardNumber = _encryptionService.EncryptText(decryptedMaskedCreditCardNumber, newEncryptionPrivateKey);
                    string encryptedCardCvv2 = _encryptionService.EncryptText(decryptedCardCvv2, newEncryptionPrivateKey);
                    string encryptedCardExpirationMonth = _encryptionService.EncryptText(decryptedCardExpirationMonth, newEncryptionPrivateKey);
                    string encryptedCardExpirationYear = _encryptionService.EncryptText(decryptedCardExpirationYear, newEncryptionPrivateKey);

                    order.CardType = encryptedCardType;
                    order.CardName = encryptedCardName;
                    order.CardNumber = encryptedCardNumber;
                    order.MaskedCreditCardNumber = encryptedMaskedCreditCardNumber;
                    order.CardCvv2 = encryptedCardCvv2;
                    order.CardExpirationMonth = encryptedCardExpirationMonth;
                    order.CardExpirationYear = encryptedCardExpirationYear;

                    // new direct debit encryption
                    string decryptedAccountHolder = _encryptionService.DecryptText(order.DirectDebitAccountHolder, oldEncryptionPrivateKey);
                    string decryptedAccountNumber = _encryptionService.DecryptText(order.DirectDebitAccountNumber, oldEncryptionPrivateKey);
                    string decryptedBankCode = _encryptionService.DecryptText(order.DirectDebitBankCode, oldEncryptionPrivateKey);
                    string decryptedBankName = _encryptionService.DecryptText(order.DirectDebitBankName, oldEncryptionPrivateKey);
                    string decryptedBic = _encryptionService.DecryptText(order.DirectDebitBIC, oldEncryptionPrivateKey);
                    string decryptedCountry = _encryptionService.DecryptText(order.DirectDebitCountry, oldEncryptionPrivateKey);
                    string decryptedIban = _encryptionService.DecryptText(order.DirectDebitIban, oldEncryptionPrivateKey);

                    string encryptedAccountHolder = _encryptionService.EncryptText(decryptedAccountHolder, newEncryptionPrivateKey);
                    string encryptedAccountNumber = _encryptionService.EncryptText(decryptedAccountNumber, newEncryptionPrivateKey);
                    string encryptedBankCode = _encryptionService.EncryptText(decryptedBankCode, newEncryptionPrivateKey);
                    string encryptedBankName = _encryptionService.EncryptText(decryptedBankName, newEncryptionPrivateKey);
                    string encryptedBic = _encryptionService.EncryptText(decryptedBic, newEncryptionPrivateKey);
                    string encryptedCountry = _encryptionService.EncryptText(decryptedCountry, newEncryptionPrivateKey);
                    string encryptedIban = _encryptionService.EncryptText(decryptedIban, newEncryptionPrivateKey);

                    order.DirectDebitAccountHolder = encryptedAccountHolder;
                    order.DirectDebitAccountNumber = encryptedAccountNumber;
                    order.DirectDebitBankCode = encryptedBankCode;
                    order.DirectDebitBankName = encryptedBankName;
                    order.DirectDebitBIC = encryptedBic;
                    order.DirectDebitCountry = encryptedCountry;
                    order.DirectDebitIban = encryptedIban;

                    _orderService.UpdateOrder(order);
                }

                //update user information
                //optimization - load only users with PasswordFormat.Encrypted
                var customers = _customerService.GetAllCustomersByPasswordFormat(PasswordFormat.Encrypted);
                foreach (var customer in customers)
                {
                    string decryptedPassword = _encryptionService.DecryptText(customer.Password, oldEncryptionPrivateKey);
                    string encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    customer.Password = encryptedPassword;
                    _customerService.UpdateCustomer(customer);
                }

                _securitySettings.EncryptionKey = newEncryptionPrivateKey;
                _settingService.SaveSetting(_securitySettings);
                SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }
            return RedirectToAction("GeneralCommon", new { selectedTab = "security" });
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("togglefulltext")]
        public ActionResult ToggleFullText(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            try
            {
                if (! _fulltextService.IsFullTextSupported())
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.NotSupported"));

                if (_commonSettings.UseFullTextSearch)
                {
                    _fulltextService.DisableFullText();

                    _commonSettings.UseFullTextSearch = false;
                    _settingService.SaveSetting(_commonSettings);

                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disabled"));
                }
                else
                {
                    _fulltextService.EnableFullText();

                    _commonSettings.UseFullTextSearch = true;
                    _settingService.SaveSetting(_commonSettings);

                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enabled"));
                }
            }
            catch (Exception exc)
            {
                ErrorNotification(exc);
            }
            return RedirectToAction("GeneralCommon", new { selectedTab = "fulltext" });
        }




        //all settings
        public ActionResult AllSettings()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            
            return View();
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult AllSettings(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var settings = _settingService
                .GetAllSettings()
				.Select(x =>
				{
					string storeName = "";
					if (x.StoreId == 0)
					{
						storeName = _localizationService.GetResource("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");
					}
					else
					{
						var store = _storeService.GetStoreById(x.StoreId);
						storeName = store != null ? store.Name : "Unknown";
					}
					var settingModel = new SettingModel()
					{
						Id = x.Id,
						Name = x.Name,
						Value = x.Value,
						Store = storeName,
						StoreId = x.StoreId
					};
					return settingModel;
				})
                .ForCommand(command)
                .ToList();
            
            var model = new GridModel<SettingModel>
            {
                Data = settings.PagedForCommand(command),
                Total = settings.Count
            };
            return new JsonResult
            {
                Data = model
            };
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingUpdate(SettingModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var setting = _settingService.GetSettingById(model.Id);
			if (setting == null)
				return Content(_localizationService.GetResource("Admin.Configuration.Settings.NoneWithThatId"));

			var storeId = Int32.Parse(model.Store); //use Store property (not StoreId) because appropriate property is stored in it

			if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) ||
				setting.StoreId != storeId)
			{
				//setting name or store has been changed
				_settingService.DeleteSetting(setting);
			}

			_settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingAdd([Bind(Exclude = "Id")] SettingModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

			var storeId = Int32.Parse(model.Store); //use Store property (not StoreId) because appropriate property is stored in it
			_settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("AddNewSetting", _localizationService.GetResource("ActivityLog.AddNewSetting"), model.Name);

            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var setting = _settingService.GetSettingById(id);
            if (setting == null)
                throw new ArgumentException("No setting found with the specified id");
            _settingService.DeleteSetting(setting);

            //activity log
            _customerActivityService.InsertActivity("DeleteSetting", _localizationService.GetResource("ActivityLog.DeleteSetting"), setting.Name);

            return AllSettings(command);
        }

        #endregion
    }
}
