﻿// Copyright 2020-2022 Andreas Atteneder
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using GLTFast.Schema;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GLTFast
{
    class CustomExtensionContractResolver : DefaultContractResolver
    {
        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var objectContract = base.CreateObjectContract(objectType);

            if (typeof(ExtensibleObject).IsAssignableFrom(objectType))
            {
                objectContract.ExtensionDataSetter = (o, key, value) =>
                {
                    AddExtension((ExtensibleObject) o, key, value);
                };

                objectContract.ExtensionDataGetter = o => GetExtensions((ExtensibleObject) o);
            }

            return objectContract;
        }

        void AddExtension(ExtensibleObject extensible, string key, object jValue)
        {
            var valueType = CustomPropertyRegistry.GetPropertyType(extensible.GetType(), key);
            var jToken = JToken.FromObject(jValue);

            extensible.genericProperties ??= new Dictionary<string, object>();

            if (valueType == null)
            {
                var value = new NewtonSoftGltfToken(jToken);
                extensible.genericProperties.Add(key, value);
            }
            else
            {
                var value = jToken.ToObject(valueType);
                extensible.genericProperties.Add(key, value);   
            }
        }

        IEnumerable<KeyValuePair<object, object>> GetExtensions(ExtensibleObject extensibleObject)
        {
            // TODO: seems very inefficient review how ExtensibleObject store its properties
            //       or find a better way to handle this with NewtonSoft

            if (extensibleObject.genericProperties == null)
                yield break;

            foreach (var property in extensibleObject.genericProperties)
            {
                if (property.Value is NewtonSoftGltfToken gltfToken)
                    yield return KeyValuePair.Create<object, object>(property.Key, gltfToken.innerObject);

                yield return KeyValuePair.Create<object, object>(property.Key, property.Value);
            }
        }
    }
}