// SCRYFALL_PROXY
// This script should be run in a Cloudflare Worker as in https://github.com/gogurt1984/Tabletop-MTG---Gogurts-DIY-Table
// See https://dash.cloudflare.com/efde9a0a4a2556bdd72677d274c246cf/workers/services/view/lucky-pond-4f97/production/settings
// Make sure the cache is enabled in runtime settings. See https://developers.cloudflare.com/workers/cache/

export default {
    async fetch(request, env, ctx) {
        const url = new URL(request.url);

        // 1. Construct the target Scryfall URL using the incoming path and query params
        const targetUrl = `https://cards.scryfall.io${url.pathname}${url.search}`;

        // 2. Make the upstream request with only a custom user agent. This is important because scryfall is actively
        // blocking TTS so we shouldn't look like that. Unlike TTS, we are caching responses so scryfall should be happy:
        // https://www.reddit.com/r/tabletopsimulator/comments/1ugfrm8/unknown_format_when_importing_from_scryfall/
        const newHeaders = new Headers();
        newHeaders.set("User-Agent", "TabletopSimulatorMtgImporter-ScryfallProxy");
        const modifiedRequest = new Request(targetUrl, {
            method: request.method,
            headers: newHeaders,
            redirect: "follow"
        });

        // Scryfall should return good cache headers, so we can just use those
        return fetch(modifiedRequest);

        // To override the cache headers
        //try {
        //    // 3. Fetch the asset from Scryfall
        //    const response = await fetch(modifiedRequest);

        //    // 4. Only alter caching headers for successful responses (HTTP 200-299)
        //    if (response.ok) {
        //        // Create a copy of the response so we can modify its headers
        //        const cachedResponse = new Response(response.body, response);

        //        // s-maxage=691200 is for Cloudflare's CDN
        //        cachedResponse.headers.set(
        //            "Cache-Control",
        //            "public, max-age=691200, s-maxage=691200" // 8 days
        //        );

        //        return cachedResponse;
        //    }

        //    // If it's an error (404, 500, etc.), pass it through without forcing a 1-week cache
        //    return response;

        //} catch (error) {
        //    return new Response("Proxy error fetching from Scryfall", { status: 502 });
        //}
    },
};