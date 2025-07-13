// Function to toggle sidebar visibility for mobile
document.getElementById("menu-toggle").addEventListener("click", function () {
    document.getElementById("sidebar-wrapper").classList.toggle("d-none");
});

// Function to show/hide the menu toggle button based on screen width
function adjustMenuToggleButton() {
    if (window.innerWidth <= 768) {
        document.getElementById("menu-toggle").style.display = "block"; // Show button on small screens
    } else {
        document.getElementById("menu-toggle").style.display = "none"; // Hide button on larger screens
    }
}

// Initial check when page loads
adjustMenuToggleButton();

// Adjust visibility of the toggle button when the window is resized
window.addEventListener("resize", adjustMenuToggleButton);
