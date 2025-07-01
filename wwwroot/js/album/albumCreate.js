// Initialize Lucide icons and image preview functionality
document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();

  const coverUpload = document.getElementById("cover-upload");
  const coverPreview = document.getElementById("cover-preview");
  const coverImage = document.getElementById("cover-image");
  const coverPlaceholder = document.getElementById("cover-placeholder");
  const removeCoverBtn = document.getElementById("remove-cover");
  const coverUrlInput = document.querySelector('input[name="CoverImageUrl"]');

  // File upload handler
  coverUpload.addEventListener("change", function (e) {
    const file = e.target.files[0];
    if (file) {
      // Validate file type
      if (!file.type.startsWith("image/")) {
        alert("Please select an image file");
        return;
      }

      // Validate file size (5MB)
      if (file.size > 5 * 1024 * 1024) {
        alert("File size must be less than 5MB");
        return;
      }

      const reader = new FileReader();
      reader.onload = function (e) {
        showImagePreview(e.target.result);
        coverUrlInput.value = ""; // Clear URL input
      };
      reader.readAsDataURL(file);
    }
  });

  // URL input handler
  coverUrlInput.addEventListener("input", function () {
    if (this.value) {
      showImagePreview(this.value);
      coverUpload.value = ""; // Clear file input
    }
  });

  // Remove cover handler
  removeCoverBtn.addEventListener("click", function () {
    hideImagePreview();
    coverUpload.value = "";
    coverUrlInput.value = "";
  });
  function showImagePreview(src) {
    coverImage.src = src;
    coverImage.classList.remove("hidden");
    coverPlaceholder.classList.add("hidden");
    removeCoverBtn.classList.remove("hidden");
    removeCoverBtn.classList.add("flex");
  }

  function hideImagePreview() {
    coverImage.classList.add("hidden");
    coverPlaceholder.classList.remove("hidden");
    removeCoverBtn.classList.add("hidden");
    removeCoverBtn.classList.remove("flex");
    coverImage.src = "";
  }
  // Form validation enhancement
  const form = document.querySelector("form");
  form.addEventListener("submit", function (e) {
    const title = document.querySelector('input[name="Title"]').value.trim();
    if (!title) {
      e.preventDefault();
      alert("Album title is required");
      return;
    }
  });

  // Smooth animations
  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  };

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("animate-fade-in-up");
      }
    });
  }, observerOptions);

  // Observe form sections
  document.querySelectorAll(".space-y-6, .space-y-8").forEach((section) => {
    observer.observe(section);
  });
});
