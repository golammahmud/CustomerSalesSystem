import axios from "https://cdn.skypack.dev/axios";
const API_BASE = "https://localhost:7120/api/";

// Update your axios instance
const apiClient = axios.create({
    baseURL: API_BASE,
    withCredentials: true,
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest',
        'X-CSRF-TOKEN': document.cookie
            .split('; ')
            .find(row => row.startsWith('CSRF-TOKEN='))
            ?.split('=')[1] || ''
    }
});

// Add CSRF token handling
apiClient.interceptors.request.use(config => {
    const csrfToken = document.cookie
        .split('; ')
        .find(row => row.startsWith('CSRF-TOKEN='))
        ?.split('=')[1];
    if (csrfToken) {
        config.headers['X-CSRF-TOKEN'] = csrfToken;
    }
    return config;
});

// Response interceptor for token refresh
let isRefreshing = false;
let refreshSubscribers = [];

function onRefreshed() {
    refreshSubscribers.forEach((cb) => cb());
    refreshSubscribers = [];
}

apiClient.interceptors.response.use(
    (response) => response,
    async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
            originalRequest._retry = true;

            if (!isRefreshing) {
                isRefreshing = true;
                try {
                    // Call refresh endpoint - cookies will be sent automatically
                    await axios.post(`${API_BASE}user/refresh`, {}, {
                        withCredentials: true
                    });

                    isRefreshing = false;
                    onRefreshed();
                } catch (err) {
                    isRefreshing = false;
                    debugger;
                    // Handle failed refresh (redirect to login, etc.)
                    //window.location.href = '/Security/login';
                    return Promise.reject(err);
                }
            }

            return new Promise((resolve) => {
                refreshSubscribers.push(() => {
                    resolve(apiClient(originalRequest));
                });
            });
        }

        return Promise.reject(error);
    }
);

//// Helper function to clear auth state
//export const logout = () => {
//    return apiClient.post('/Security/login?handler=LogOff', {}, { withCredentials: true })
//        .then(() => {
//            // Redirect to login page after logout
//            window.location.href = '/Security/login';
//        });
//};

export default apiClient;