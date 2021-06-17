using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TabletopMtgImporter.Blazor
{
    public class DownloadSaver : ISaver
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILogger _logger;

        public DownloadSaver(IJSRuntime jsRuntime, ILogger logger)
        {
            this._jsRuntime = jsRuntime;
            this._logger = logger;
        }

        public async Task SaveAsync(string name, string contents)
        {
            // based on https://wellsb.com/csharp/aspnet/blazor-jsinterop-save-file/
            await this._jsRuntime.InvokeVoidAsync("eval", $@"
                var link = document.createElement('a');
                link.download = {JsonConvert.SerializeObject(name)};
                link.saveAs = true;
                link.href = 'data:text/plain;charset=utf-8,' + encodeURIComponent({JsonConvert.SerializeObject(contents)});
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            ");

            this._logger.Info($"Downloaded {name}");
        }
    }
}
