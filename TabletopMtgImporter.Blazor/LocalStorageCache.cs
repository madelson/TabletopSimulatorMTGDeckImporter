using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Blazor
{
    public class LocalStorageCache : ICache
    {
        private readonly IJSRuntime _jsRuntime;

        public LocalStorageCache(IJSRuntime jsRuntime)
        {
            this._jsRuntime = jsRuntime;
        }

        // based loosely on https://github.com/Blazored/LocalStorage/blob/main/src/Blazored.LocalStorage/BrowserStorageProvider.cs

        public Task<string?> GetValueOrDefaultAsync(string key) =>
            this._jsRuntime.InvokeAsync<string?>("localStorage.getItem", key).AsTask();

        public Task SetValueAsync(string key, string value) =>
            this._jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value).AsTask();
    }
}
