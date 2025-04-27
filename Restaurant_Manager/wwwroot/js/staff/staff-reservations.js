// (EN) Change reservation status| (BG) Променя статуса на резервация
function changeReservationStatus(reservationId, selectElement) {
    const newStatus = selectElement.value;

    fetch(`/Staff/UpdateReservationStatus?id=${reservationId}&newStatus=${newStatus}`, {
        method: 'POST'
    })
        .then(response => {
            if (response.ok) {
                location.reload(); 
            } else {
                alert('Failed to update reservation status.');
            }
        })
        .catch(error => {
            console.error('Error updating reservation:', error);
        });
}
