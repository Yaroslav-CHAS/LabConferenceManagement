window.conferenceApi = {
    async send(method, path, body, csrfToken) {
        const headers = { Accept: "application/json" };
        if (body !== null) headers["Content-Type"] = "application/json";
        if (method !== "GET" && csrfToken) headers["X-CSRF-TOKEN"] = csrfToken;
        try {
            const response = await fetch(path, { method, body, headers, credentials: "same-origin" });
            return {
                status: response.status,
                body: await response.text(),
                contentType: response.headers.get("Content-Type") || ""
            };
        } catch {
            return { status: 0, body: "", contentType: "" };
        }
    }
};
