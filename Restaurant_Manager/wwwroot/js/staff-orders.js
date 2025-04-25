function changeOrderStatus(orderId, selectElement) {
    const newStatus = selectElement.value;

    fetch(`/Staff/UpdateOrderStatus?id=${orderId}&newStatus=${newStatus}`, {
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
