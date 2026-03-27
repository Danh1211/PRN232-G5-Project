// helper functions for frontend
function getToken() {
    return localStorage.getItem('token') || '';
}

function parseJwt(token) {
    try {
        const payload = token.split('.')[1];
        const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
        return JSON.parse(decoded);
    } catch {
        return null;
    }
}

async function postFormData(url, formData) {
    const token = getToken();
    const headers = token ? { 'Authorization': 'Bearer ' + token } : {};
    const res = await fetch(url, {
        method: 'POST',
        headers: headers,
        body: formData
    });
    return res;
}

async function putJson(url, json) {
    const token = getToken();
    const res = await fetch(url, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            ...(token ? { 'Authorization': 'Bearer ' + token } : {})
        },
        body: JSON.stringify(json)
    });
    return res;
}

async function getJson(url) {
    const token = getToken();
    const res = await fetch(url, {
        method: 'GET',
        headers: {
            'Accept': 'application/json',
            ...(token ? { 'Authorization': 'Bearer ' + token } : {})
        }
    });
    return res;
}