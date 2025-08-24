// wwwroot/js/toaster.js
export const ToastType = {
    Success: 'success',
    Info: 'info',
    Warning: 'warning',
    Error: 'error'
};

export function showToast(message, type = ToastType.Info, title = '') {
    if (!message) return;

    // Optional: configure toastr globally
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-bottom-right",
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    switch (type) {
        case ToastType.Success:
            toastr.success(message, title);
            break;
        case ToastType.Info:
            toastr.info(message, title);
            break;
        case ToastType.Warning:
            toastr.warning(message, title);
            break;
        case ToastType.Error:
            toastr.error(message, title);
            break;
        default:
            toastr.info(message, title);
            break;
    }
}

// Optional helper to read TempData from Razor pages
export function showTempDataToast(tempData) {
    if (!tempData) return;
    const { ToastMessage, ToastType, ToastTitle } = tempData;
    if (ToastMessage) {
        showToast(ToastMessage, ToastType || ToastType.Info, ToastTitle || '');
    }
}
