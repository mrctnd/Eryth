document.addEventListener("DOMContentLoaded", function () {
  const fileDropZone = document.getElementById("file-drop-zone");
  const audioFileInput = document.getElementById("audio-file-input");
  const metadataForm = document.getElementById("metadata-form");
  const backBtn = document.getElementById("back-btn");
  const coverImageInput = document.getElementById("cover-image-input");
  const coverImagePreview = document.getElementById("cover-image-preview");
  const removeCoverBtn = document.getElementById("remove-cover");

  // File drag and drop
  fileDropZone.addEventListener("dragover", (e) => {
    e.preventDefault();
    fileDropZone.classList.add("border-accent", "bg-accent/5");
  });

  fileDropZone.addEventListener("dragleave", (e) => {
    e.preventDefault();
    fileDropZone.classList.remove("border-accent", "bg-accent/5");
  });
  fileDropZone.addEventListener("drop", (e) => {
    e.preventDefault();
    fileDropZone.classList.remove("border-accent", "bg-accent/5");

    const files = e.dataTransfer.files;
    if (files.length > 0) {
      handleFileSelection(files[0]);
    }
  });

  // File input change
  audioFileInput.addEventListener("change", (e) => {
    if (e.target.files.length > 0) {
      handleFileSelection(e.target.files[0]);
    }
  });

  // Handle file selection
  function handleFileSelection(file) {
    // Validate file type
    const allowedTypes = [".mp3", ".wav", ".flac", ".m4a", ".aiff", ".alac"];
    const fileExtension = "." + file.name.split(".").pop().toLowerCase();

    if (!allowedTypes.includes(fileExtension)) {      alert(
        "Lütfen geçerli bir ses dosyası seçin (MP3, WAV, FLAC, M4A, AIFF, ALAC)"
      );
      return;
    }

    // Validate file size (4GB = 4 * 1024 * 1024 * 1024 bytes)
    const maxSize = 4 * 1024 * 1024 * 1024;
    if (file.size > maxSize) {
      alert("Dosya boyutu 4GB'dan az olmalıdır");
      return;
    }

    // Hide upload area
    fileDropZone.style.display = "none";

    // Auto-fill title and artist from filename if empty
    const titleInput = document.querySelector('input[name="Title"]');
    const artistInput = document.querySelector('input[name="Artist"]');

    if (titleInput && artistInput) {
      if (!titleInput.value || !artistInput.value) {
        // Try to extract metadata from file (this is a basic approach)
        const fileName = file.name.split(".").slice(0, -1).join(".");

        // Basic pattern matching for "Artist - Title" format
        const dashIndex = fileName.indexOf(" - ");
        if (dashIndex > 0) {
          const extractedArtist = fileName.substring(0, dashIndex).trim();
          const extractedTitle = fileName.substring(dashIndex + 3).trim();

          if (!artistInput.value) artistInput.value = extractedArtist;
          if (!titleInput.value) titleInput.value = extractedTitle;
        } else {
          // If no dash pattern, use filename as title
          if (!titleInput.value) titleInput.value = fileName;
        }
      }
    }

    // Automatically show metadata form after a short delay
    setTimeout(() => {
      metadataForm.classList.remove("hidden");
    }, 300);
  }

  // Back to file selection
  backBtn.addEventListener("click", () => {
    metadataForm.classList.add("hidden");
    fileDropZone.style.display = "block";
    audioFileInput.value = "";
  });

  // Cover image handling
  coverImageInput.addEventListener("change", (e) => {
    if (e.target.files.length > 0) {
      const file = e.target.files[0];

      // Validate image file
      if (!file.type.startsWith("image/")) {
        alert("Lütfen geçerli bir resim dosyası seçin");
        return;
      }

      const reader = new FileReader();
      reader.onload = (e) => {
        coverImagePreview.innerHTML = `<img src="${e.target.result}" class="w-full h-full object-cover rounded-lg" alt="Cover preview">`;
        removeCoverBtn.classList.remove("hidden");
      };
      reader.readAsDataURL(file);
    }
  });

  // Remove cover image
  removeCoverBtn.addEventListener("click", () => {
    coverImageInput.value = "";
    coverImagePreview.innerHTML = `
            <div class="text-center">
                <i data-lucide="image" class="w-8 h-8 text-gray-400 mx-auto mb-2"></i>
                <p class="text-gray-400 text-sm">Resim yükle</p>
            </div>
        `;
    removeCoverBtn.classList.add("hidden");

    // Re-initialize lucide icons
    lucide.createIcons();
  });

  // Format file size
  function formatFileSize(bytes) {
    if (bytes === 0) return "0 Bytes";
    const k = 1024;
    const sizes = ["Byte", "KB", "MB", "GB"];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
  }

  // Initialize lucide icons
  lucide.createIcons();
});
