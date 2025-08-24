const API_BASE = "https://localhost:7120/api/";

async function fetchClient(url, options = {}) {
    const defaultOptions = {
        credentials: 'include',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    };

    const finalOptions = { ...defaultOptions, ...options };
    finalOptions._retry = finalOptions._retry || false; // add retry flag

    const response = await fetch(`${API_BASE}${url}`, finalOptions);

    if (response.status === 401 && !finalOptions._retry) {
        finalOptions._retry = true; // prevent infinite retry

        const refreshResponse = await fetch(`${API_BASE}user/refresh`, {
            method: 'POST',
            credentials: 'include',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (refreshResponse.ok) {
            // Retry original request once
            return fetchClient(url, finalOptions);
        } else {
            throw new Error('Session expired. Please log in again.');
        }
    }

    if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
    }

    return response.json();
}

// Customer API
export const customerAPI = {
    getAll: (pageNumber, pageSize) => fetchClient(`customers?pageNumber=${pageNumber}&pageSize=${pageSize}`),
    getById: (id) => fetchClient(`customers/${id}`),
    create: (data) => fetchClient(`customers`, { method: 'POST', body: JSON.stringify(data) }),
    update: (id, data) => fetchClient(`customers/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id) => fetchClient(`customers/${id}`, { method: 'DELETE' }),
};

// Sales API
export const salesAPI = {
    getAll: () => fetchClient(`sales`),
    getById: (id) => fetchClient(`sales/${id}`),
    create: (data) => fetchClient(`sales`, { method: 'POST', body: JSON.stringify(data) }),
    delete: (id) => fetchClient(`sales/${id}`, { method: 'DELETE' }),
    filter: (customerId, date) => fetchClient(`sales/filter?customerId=${customerId}&date=${date}`)
};

// Product API
export const productAPI = {
    getAll: () => fetchClient(`products`),
    getById: (id) => fetchClient(`products/${id}`),
    create: (data) => fetchClient(`products`, { method: 'POST', body: JSON.stringify(data) }),
    update: (id, data) => fetchClient(`products/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
    delete: (id) => fetchClient(`products/${id}`, { method: 'DELETE' }),
};
