import axios from 'https://cdn.skypack.dev/axios';

const API_BASE = 'https://localhost:7120/api';



export const customerAPI = {
    getAll: (pageNumber, pageSize) => axios.get(`${API_BASE}/customers`, { params: { pageNumber, pageSize }}),
    getById: id => axios.get(`${API_BASE}/customers/${id}`),
    create: data => axios.post(`${API_BASE}/customers`, data),
    update: (id, data) => axios.put(`${API_BASE}/customers/${id}`, data),
    delete: id => axios.delete(`${API_BASE}/customers/${id}`)
};

export const salesAPI = {
    getAll: () => axios.get(`${API_BASE}/Sales`),
    getById: id => axios.get(`${API_BASE}/Sales/${id}`),
    create: data => axios.post(`${API_BASE}/Sales`, data),
    delete: id => axios.delete(`${API_BASE}/Sales/${id}`),
    filter: (customerId, date) =>
        axios.get(`${API_BASE}/Sales/filter`, { params: { customerId, date } })
};
export const productAPI = {
    getAll: () => axios.get(`${API_BASE}/products`),
    getById: id => axios.get(`${API_BASE}/products/${id}`),
    create: data => axios.post(`${API_BASE}/products`, data),
    update: (id, data) => axios.put(`${API_BASE}/products/${id}`, data),
    delete: id => axios.delete(`${API_BASE}/products/${id}`)
};
