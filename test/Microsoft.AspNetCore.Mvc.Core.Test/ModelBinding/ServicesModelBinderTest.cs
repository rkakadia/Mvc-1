// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    public class ServicesModelBinderTest
    {
        [Fact]
        public async Task ServiceModelBinder_BindsService()
        {
            // Arrange
            var type = typeof(IService);

            var binder = new ServicesModelBinder();
            var modelBindingContext = GetBindingContext(type);

            // Act
            var result = await binder.BindModelResultAsync(modelBindingContext);

            // Assert
            Assert.NotEqual(default(ModelBindingResult), result);
            Assert.True(result.IsModelSet);
            Assert.NotNull(result.Model);
            Assert.Equal("modelName", result.Key);

            var entry = modelBindingContext.ValidationState[result.Model];
            Assert.True(entry.SuppressValidation);
            Assert.Null(entry.Key);
            Assert.Null(entry.Metadata);
        }

        [Fact]
        public async Task ServiceModelBinder_ReturnsNothing_ForNullBindingSource()
        {
            // Arrange
            var type = typeof(IService);

            var binder = new ServicesModelBinder();
            var modelBindingContext = GetBindingContext(type);
            modelBindingContext.BindingSource = null;

            // Act
            var result = await binder.BindModelResultAsync(modelBindingContext);

            // Assert
            Assert.Equal(default(ModelBindingResult), result);
        }

        [Fact]
        public async Task ServiceModelBinder_ReturnsNothing_ForNonServiceBindingSource()
        {
            // Arrange
            var type = typeof(IService);

            var binder = new ServicesModelBinder();
            var modelBindingContext = GetBindingContext(type);
            modelBindingContext.BindingSource = BindingSource.Body;

            // Act
            var result = await binder.BindModelResultAsync(modelBindingContext);

            // Assert
            Assert.Equal(default(ModelBindingResult), result);
        }

        private static DefaultModelBindingContext GetBindingContext(Type modelType)
        {
            var metadataProvider = new TestModelMetadataProvider();
            metadataProvider.ForType(modelType).BindingDetails(d => d.BindingSource = BindingSource.Services);
            var modelMetadata = metadataProvider.GetMetadataForType(modelType);


            var services = new ServiceCollection();
            services.AddSingleton<IService>(new Service());

            var bindingContext = new DefaultModelBindingContext
            {
                ModelMetadata = modelMetadata,
                ModelName = "modelName",
                FieldName = "modelName",
                ModelState = new ModelStateDictionary(),
                OperationBindingContext = new OperationBindingContext
                {
                    ActionContext = new ActionContext()
                    {
                        HttpContext = new DefaultHttpContext()
                        {
                            RequestServices = services.BuildServiceProvider(),
                        }
                    },
                    ModelBinder = new HeaderModelBinder(),
                    MetadataProvider = metadataProvider,
                },
                BinderModelName = modelMetadata.BinderModelName,
                BindingSource = modelMetadata.BindingSource,
                ValidationState = new ValidationStateDictionary(),
            };

            return bindingContext;
        }

        private interface IService
        {
        }

        private class Service : IService
        {
        }
    }
}
