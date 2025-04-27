// (EN) Change order status| (BG) Променя статуса на поръчка
function changeOrderStatus(orderId, selectElement) {
    const newStatus = selectElement.value;

    fetch(`/Admin/UpdateOrderStatus?id=${orderId}&newStatus=${newStatus}`, {
        method: 'POST'
    })
        .then(response => {
            if (response.ok) {
                location.reload();
            } else {
                alert('Failed to update order status.');
            }
        })
        .catch(error => {
            console.error('Error updating order:', error);
        });
}


