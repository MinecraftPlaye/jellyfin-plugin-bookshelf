const BookshelfConfig = {
    pluginUniqueId: '9c4e63f1-031b-4f25-988b-4f7d78a8b53e'
};

export default function (view, params) {
    view.addEventListener('viewshow', function () {
        Dashboard.showLoadingMsg();
        const page = this;
        ApiClient.getPluginConfiguration(BookshelfConfig.pluginUniqueId).then(function (config) {
            page.querySelector('#comicvineapikey').value = config.ApiKey || '';
            Dashboard.hideLoadingMsg();
        });
    });

    view.querySelector('#ComicVineConfigForm').addEventListener('submit', function (e) {
        e.preventDefault();
        Dashboard.showLoadingMsg();
        const form = this;
        ApiClient.getPluginConfiguration(BookshelfConfig.pluginUniqueId).then(function (config) {
            const apiKey = form.querySelector('#comicvineapikey').value;

            if (!apiKey) {
                Dashboard.processErrorResponse({ statusText: "ComicVine API key is missing" });
                return;
            }

            const el = form.querySelector('#ossresponse');

            const data = JSON.stringify({ ApiKey: apiKey });
            const url = ApiClient.getUrl('Jellyfin.Plugin.Bookshelf/ValidateComicVineApiKey');

            const handler = response => response.json().then(res => {
                if (response.ok) {
                    el.innerText = `Login info validated, this account can download ${res.Downloads} subtitles per hour`;

                    config.ApiKey = apiKey;

                    ApiClient.updatePluginConfiguration(BookshelfConfig.pluginUniqueId, config).then(function (result) {
                        Dashboard.processPluginConfigurationUpdateResult(result);
                    });
                }
                else {
                    let msg = res.Message ?? JSON.stringify(res, null, 2);

                    if (msg == 'You cannot consume this service') {
                        msg = 'Invalid API key provided';
                    }

                    Dashboard.processErrorResponse({ statusText: `Request failed - ${msg}` });
                }
            });

            ApiClient.ajax({ type: 'POST', url, data, contentType: 'application/json' }).then(handler).catch(handler);
        });
        return false;
    });
}
