import apiClient from "./apiClient.js"; // note the .js extension

export const customerAPI = {
    getAll: (pageNumber, pageSize) => apiClient.get(`/customers`, { params: { pageNumber, pageSize } }),
    getById: (id) => apiClient.get(`/customers/${id}`),
    create: (data) => apiClient.post(`/customers`, data),
    update: (id, data) => apiClient.put(`/customers/${id}`, data),
    delete: (id) => apiClient.delete(`/customers/${id}`),
};

export const salesAPI = {
    getAll: () => apiClient.get(`/sales`),
    getById: (id) => apiClient.get(`/sales/${id}`),
    create: (data) => apiClient.post(`/sales`, data),
    delete: (id) => apiClient.delete(`/sales/${id}`),
    filter: (customerId, date) => apiClient.get(`/sales/filter`, { params: { customerId, date } }),
};

export const productAPI = {
    getAll: () => apiClient.get(`/products`),
    getById: (id) => apiClient.get(`/products/${id}`),
    create: (data) => apiClient.post(`/products`, data),
    update: (id, data) => apiClient.put(`/products/${id}`, data),
    delete: (id) => apiClient.delete(`/products/${id}`),
};
//export const customerAPI = {
//    getAll: (pageNumber, pageSize) => apiClient.get(`${API_BASE}/customers`, { params: { pageNumber, pageSize }}),
//    getById: id => apiClient.get(`${API_BASE}/customers/${id}`),
//    create: data => apiClient.post(`${API_BASE}/customers`, data),
//    update: (id, data) => apiClient.put(`${API_BASE}/customers/${id}`, data),
//    delete: id => apiClient.delete(`${API_BASE}/customers/${id}`)
//};

//export const salesAPI = {
//    getAll: () => apiClient.get(`${API_BASE}/Sales`),
//    getById: id => apiClient.get(`${API_BASE}/Sales/${id}`),
//    create: data => apiClient.post(`${API_BASE}/Sales`, data),
//    delete: id => apiClient.delete(`${API_BASE}/Sales/${id}`),
//    filter: (customerId, date) =>
//        apiClient.get(`${API_BASE}/Sales/filter`, { params: { customerId, date } })
//};
//export const productAPI = {
//    getAll: () => apiClient.get(`${API_BASE}/products`),
//    getById: id => apiClient.get(`${API_BASE}/products/${id}`),
//    create: data => apiClient.post(`${API_BASE}/products`, data),
//    update: (id, data) => apiClient.put(`${API_BASE}/products/${id}`, data),
//    delete: id => apiClient.delete(`${API_BASE}/products/${id}`)
//};
