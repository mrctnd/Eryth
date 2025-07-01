// Initialize Lucide icons
lucide.createIcons();

// Banner image preview functionality
const bannerInput = document.querySelector('input[name="BannerImage"]');
if (bannerInput) {
  bannerInput.addEventListener("change", function (e) {
    const file = e.target.files[0];
    const preview = document.querySelector(".banner-preview img");
    const placeholder = document.querySelector(
      ".banner-preview .banner-placeholder"
    );

    if (file) {
      const reader = new FileReader();
      reader.onload = function (e) {
        if (preview) {
          preview.src = e.target.result;
          preview.style.display = "block";
        }
        if (placeholder) {
          placeholder.style.display = "none";
        }
      };
      reader.readAsDataURL(file);
    }
  });
}

// Profile image preview functionality
const profileInput = document.querySelector('input[name="ProfileImage"]');
if (profileInput) {
  profileInput.addEventListener("change", function (e) {
    const file = e.target.files[0];
    const preview = document.querySelector(".profile-preview img");

    if (file && preview) {
      const reader = new FileReader();
      reader.onload = function (e) {
        preview.src = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  });
}

// Toggle delete account section
function toggleDeleteSection() {
  const section = document.getElementById("delete-account-section");
  const checkbox = document.querySelector(
    'input[name="RequestAccountDeletion"]'
  );

  if (section.classList.contains("hidden")) {
    section.classList.remove("hidden");
  } else {
    section.classList.add("hidden");
    checkbox.checked = false;
    document.querySelector('input[name="DeleteAccountPassword"]').value = "";
  }
}

// Unsaved changes tracking and modal functionality
let hasUnsavedChanges = false;
let isSubmitting = false;
let pendingNavigation = null; // Initialize form state - ensure we start with no unsaved changes
window.addEventListener("DOMContentLoaded", function () {
  hasUnsavedChanges = false;
  isSubmitting = false;
});

// Track changes in form fields
document.querySelectorAll("input, textarea, select").forEach((field) => {
  // Skip tracking for delete account fields as they don't count as "unsaved changes"
  if (
    field.name === "RequestAccountDeletion" ||
    field.name === "DeleteAccountPassword"
  ) {
    return;
  }

  field.addEventListener("change", function () {
    hasUnsavedChanges = true;
  });

  field.addEventListener("input", function () {
    hasUnsavedChanges = true;
  });
}); // Form validation and submission
document.querySelector("form").addEventListener("submit", function (e) {
  const deleteCheckbox = document.querySelector(
    'input[name="RequestAccountDeletion"]'
  );
  const deletePassword = document.querySelector(
    'input[name="DeleteAccountPassword"]'
  );

  if (deleteCheckbox.checked) {
    if (!deletePassword.value.trim()) {
      e.preventDefault();
      showDeleteModal("Hesabınızı silmek için şifrenizi girmelisiniz.");
      deletePassword.focus();
      return;
    }

    if (
      !confirm(
        "Hesabınızı silmek istediğinizden kesinlikle emin misiniz? Bu işlem geri alınamaz ve tüm verileriniz kalıcı olarak silinecektir."
      )
    ) {
      e.preventDefault();
      return;
    }
  }

  // Important: Remove the event listener BEFORE form submission
  window.removeEventListener("beforeunload", beforeUnloadHandler);

  // Mark as submitted to prevent other navigation warnings
  isSubmitting = true;
  hasUnsavedChanges = false;
});

// Show delete confirmation modal
function showDeleteModal(message) {
  const modal = document.createElement("div");
  modal.className =
    "fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center";
  modal.innerHTML = `
            <div class="bg-primary-light rounded-xl p-6 max-w-md w-full mx-4 border border-red-700">
                <div class="flex items-center mb-4">
                    <i data-lucide="alert-circle" class="w-5 h-5 text-red-500 mr-2"></i>
                    <h3 class="text-lg font-semibold text-white">Hata</h3>
                </div>
                <p class="text-gray-300 mb-6">${message}</p>
                <div class="flex justify-end">
                    <button onclick="this.closest('.fixed').remove()" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-lg transition-colors">
                        Tamam
                    </button>
                </div>
            </div>
        `;
  document.body.appendChild(modal);
  lucide.createIcons();
}
// Handle unsaved changes modal
function showUnsavedChangesModal() {
  const modal = document.getElementById("unsavedChangesModal");
  modal.classList.remove("hidden");
  modal.style.display = "flex";
  modal.style.alignItems = "center";
  modal.style.justifyContent = "center";
}

function hideUnsavedChangesModal() {
  const modal = document.getElementById("unsavedChangesModal");
  modal.classList.add("hidden");
  modal.style.display = "none";
}

function cancelLeave() {
  hideUnsavedChangesModal();
  pendingNavigation = null;
}

function confirmLeave() {
  hasUnsavedChanges = false;
  hideUnsavedChangesModal();
  if (pendingNavigation) {
    pendingNavigation();
  }
} // TOTAL WINDOWS ALERT KILLER - ABSOLUTE FIX!
let beforeUnloadRemoved = false;

function beforeUnloadHandler(e) {
  // Only show alert if there are unsaved changes AND we're not submitting
  if (hasUnsavedChanges && !isSubmitting && !beforeUnloadRemoved) {
    e.preventDefault();
    e.returnValue = "";
    return "";
  }
  // Return nothing to prevent any alert
  return undefined;
}

// Add the event listener
window.addEventListener("beforeunload", beforeUnloadHandler);

// NUCLEAR OPTION: Complete beforeunload removal on Save Changes
document
  .getElementById("saveChangesBtn")
  .addEventListener("click", function () {
    console.log("Save button clicked - REMOVING ALL BEFOREUNLOAD EVENTS");

    // Set flags IMMEDIATELY
    isSubmitting = true;
    hasUnsavedChanges = false;
    beforeUnloadRemoved = true;

    // Remove ALL beforeunload listeners
    window.removeEventListener("beforeunload", beforeUnloadHandler);

    // Override ANY potential beforeunload
    window.onbeforeunload = null;

    // Create a new empty beforeunload that does NOTHING
    window.addEventListener("beforeunload", function () {
      return undefined;
    });
  });

// Also remove on form submit as backup
document.querySelector("form").addEventListener("submit", function () {
  console.log("Form submitted - KILLING BEFOREUNLOAD");
  isSubmitting = true;
  hasUnsavedChanges = false;
  beforeUnloadRemoved = true;
  window.removeEventListener("beforeunload", beforeUnloadHandler);
  window.onbeforeunload = null;
});

// Intercept link clicks
document.addEventListener("click", function (e) {
  if (hasUnsavedChanges && !isSubmitting) {
    const link = e.target.closest("a");
    if (link && link.href && !link.href.startsWith("#")) {
      e.preventDefault();
      pendingNavigation = function () {
        window.location.href = link.href;
      };
      showUnsavedChangesModal();
    }
  }
}); // Reset changes button functionality
document
  .getElementById("resetChangesBtn")
  .addEventListener("click", function (e) {
    e.preventDefault();
    // Always reload without showing modal - this is intentional reset
    hasUnsavedChanges = false;
    window.location.reload();
  });
