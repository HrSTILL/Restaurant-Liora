function updateCartQuantity(menuItemId, newQuantity) {
    fetch('/Cart/UpdateQuantityAjax', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ menuItemId, quantity: newQuantity })
    })
        .then(res => res.json())
        .then(result => {
            if (result.success) {
                location.reload();
            } else {
                alert("Failed to update quantity");
            }
        });
}

async function removeFromCart(menuItemId, button) {
    const response = await fetch('/Cart/RemoveFromCartAjax', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ menuItemId })
    });

    const result = await response.json();
    if (result.success) {
        const itemElement = button.closest('.cart-item');
        if (itemElement) itemElement.remove();
        location.reload();

        if (result.newCount !== undefined) {
            const badge = document.getElementById('cartBadge');
            if (badge) {
                badge.innerText = result.newCount;
                badge.style.display = result.newCount > 0 ? "inline-block" : "none";
            }
        }
    } else {
        alert("Failed to remove item.");
    }
}