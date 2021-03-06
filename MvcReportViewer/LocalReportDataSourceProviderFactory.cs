﻿using System;
using System.Configuration;
using System.Web.Mvc;

namespace MvcReportViewer
{
    public class LocalReportDataSourceProviderFactory
    {
        private static readonly object SyncRoot = new object();

        private static LocalReportDataSourceProviderFactory _factory;

        public static LocalReportDataSourceProviderFactory Current
        {
            get
            {
                if (_factory != null)
                {
                    return _factory;
                }

                var resolver = DependencyResolver.Current;
                lock(SyncRoot)
                {
                    _factory = resolver.GetService<LocalReportDataSourceProviderFactory>() ??
                               new LocalReportDataSourceProviderFactory();
                }

                return _factory;
            }
        }

        public virtual ILocalReportDataSourceProvider Create()
        {
            var resolver = DependencyResolver.Current;
            var provider = resolver.GetService<ILocalReportDataSourceProvider>();
            if (provider != null)
            {
                return provider;
            }

            // Try to get data source provider from database settings

            var providerTypeName = ConfigurationManager.AppSettings[WebConfigSettings.LocalDataSourceProvider];
            if (string.IsNullOrEmpty(providerTypeName))
            {
                throw new MvcReportViewerException(
                    $"{WebConfigSettings.LocalDataSourceProvider} configuration is not found in the Web.config");
            }

            try
            {
                var providerType = Type.GetType(providerTypeName);
                if (providerType == null)
                {
                    throw new InvalidOperationException($"Cannot find {providerTypeName} type");    
                }

                provider = (ILocalReportDataSourceProvider)Activator.CreateInstance(providerType);

                return provider;
            }
            catch (Exception err)
            {
                throw new MvcReportViewerException(
                    $"{WebConfigSettings.LocalDataSourceProvider} configuration in the Web.config is not correct",
                    err);
            }
        }
    }
}
