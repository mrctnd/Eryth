document.addEventListener("DOMContentLoaded", function () {
  // Initialize Lucide icons
  if (typeof lucide !== "undefined") {
    lucide.createIcons();
  }

  // Add smooth scrolling for navigation
  const backButton = document.querySelector('a[href*="Profile"]');
  if (backButton) {
    backButton.addEventListener('click', function(e) {
      // Add a subtle loading animation
      this.style.transform = 'scale(0.95)';
      setTimeout(() => {
        this.style.transform = 'scale(1)';
      }, 150);
    });
  }

  // Add intersection observer for comment animations
  const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
  };

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.style.opacity = '1';
        entry.target.style.transform = 'translateY(0)';
      }
    });
  }, observerOptions);

  // Observe all comment cards
  document.querySelectorAll('[id^="comment-"]').forEach(comment => {
    comment.style.opacity = '0';
    comment.style.transform = 'translateY(20px)';
    comment.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
    observer.observe(comment);
  });

  // Character counter for edit modal
  const editTextarea = document.getElementById('edit-comment-content');
  const charCount = document.getElementById('edit-char-count');
  
  if (editTextarea && charCount) {
    editTextarea.addEventListener('input', function() {
      const length = this.value.length;
      charCount.textContent = `${length}/1000`;      if (length > 900) {
        charCount.classList.add('text-red-400');
        charCount.classList.remove('text-green-400');
      } else {
        charCount.classList.add('text-green-400');
        charCount.classList.remove('text-red-400');
      }
    });
  }

  // Handle edit form submission
  const editForm = document.getElementById('edit-comment-form');
  if (editForm) {
    editForm.addEventListener('submit', function(e) {
      e.preventDefault();
      submitEditComment();
    });
  }

  // Close modal when clicking outside
  const editModal = document.getElementById('edit-comment-modal');
  if (editModal) {
    editModal.addEventListener('click', function(e) {
      if (e.target === this) {
        closeEditModal();
      }
    });
  }
});

// Edit Comment Modal Functions
function openEditModal(commentId, currentContent) {
  console.log('Opening edit modal for comment:', commentId);
  
  const modal = document.getElementById('edit-comment-modal');
  const textarea = document.getElementById('edit-comment-content');
  const commentIdInput = document.getElementById('edit-comment-id');
  const charCount = document.getElementById('edit-char-count');
  
  if (modal && textarea && commentIdInput) {
    // Set values - decode HTML entities
    commentIdInput.value = commentId;
    
    // Decode HTML entities from the content
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = currentContent;
    const decodedContent = tempDiv.textContent || tempDiv.innerText || '';
    
    textarea.value = decodedContent;
    
    // Update character count
    if (charCount) {
      const length = decodedContent.length;
      charCount.textContent = `${length}/1000`;
      
      // Update color based on length
      if (length > 900) {
        charCount.classList.add('text-red-400');
        charCount.classList.remove('text-green-400');
      } else {
        charCount.classList.add('text-green-400');
        charCount.classList.remove('text-red-400');
      }
    }
    
    // Show modal with animation
    modal.classList.remove('hidden');
    modal.style.display = 'flex';
    modal.style.opacity = '0';
    modal.offsetHeight; // Force reflow
    modal.style.transition = 'opacity 0.3s ease';
    modal.style.opacity = '1';
    
    // Focus textarea
    setTimeout(() => {
      textarea.focus();
      textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    }, 100);
    
    // Close any open dropdowns
    document.querySelectorAll('[id^="comment-menu-"]').forEach(menu => {
      menu.classList.add('hidden');
    });
  }
}

function closeEditModal() {
  const modal = document.getElementById('edit-comment-modal');
  if (modal) {
    modal.style.opacity = '0';
    setTimeout(() => {
      modal.classList.add('hidden');
      modal.style.display = 'none';
      // Clear form
      document.getElementById('edit-comment-content').value = '';
      document.getElementById('edit-comment-id').value = '';
    }, 300);
  }
}

function submitEditComment() {
  const commentId = document.getElementById('edit-comment-id').value;
  const newContent = document.getElementById('edit-comment-content').value.trim();
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
  
  if (!newContent) {
    showToast('Comment cannot be empty', 'error');
    return;
  }
  
  if (newContent.length > 1000) {
    showToast('Comment is too long (max 1000 characters)', 'error');
    return;
  }
  
  console.log('Submitting edit for comment:', commentId);
  
  // Show loading state
  const submitBtn = document.querySelector('#edit-comment-form button[type="submit"]');
  const originalText = submitBtn.innerHTML;
  submitBtn.innerHTML = '<i data-lucide="loader-2" class="w-4 h-4 animate-spin"></i><span>Saving...</span>';
  submitBtn.disabled = true;
  
  fetch(`/Comment/Edit/${commentId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'RequestVerificationToken': token,
      'X-Requested-With': 'XMLHttpRequest'
    },
    body: JSON.stringify(newContent)
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  })
  .then(data => {
    if (data.success) {
      // Update the comment content in the page
      const commentContentDiv = document.getElementById(`comment-content-${commentId}`);
      if (commentContentDiv) {
        const paragraph = commentContentDiv.querySelector('p');
        if (paragraph) {
          paragraph.textContent = newContent;
          
          // Add visual feedback with pulse effect
          commentContentDiv.style.background = 'rgba(16, 185, 129, 0.3)';
          commentContentDiv.style.transform = 'scale(1.02)';
          commentContentDiv.style.transition = 'all 0.3s ease';
          
          setTimeout(() => {
            commentContentDiv.style.background = 'rgba(0, 0, 0, 0.4)';
            commentContentDiv.style.transform = 'scale(1)';
          }, 2000);
          
          // Add "edited" indicator if not already present
          const timestampDiv = commentContentDiv.closest('[id^="comment-"]').querySelector('.text-green-200');
          if (timestampDiv && !timestampDiv.textContent.includes('• edited')) {
            const editedSpan = document.createElement('span');
            editedSpan.className = 'text-green-400 italic';
            editedSpan.textContent = '• edited';
            timestampDiv.appendChild(editedSpan);
          }
        }
      }
      
      showToast('Comment updated successfully! ✏️', 'success');
      closeEditModal();
    } else {
      showToast(data.message || 'Failed to update comment', 'error');
    }
  })
  .catch(error => {
    console.error('Error updating comment:', error);
    showToast('An error occurred while updating the comment. Please try again.', 'error');
  })
  .finally(() => {
    // Restore button state
    submitBtn.innerHTML = originalText;
    submitBtn.disabled = false;
    
    // Re-initialize Lucide icons
    if (typeof lucide !== 'undefined') {
      lucide.createIcons();
    }
  });
}

// Delete Comment Function
function deleteComment(commentId) {
  console.log('Deleting comment:', commentId);
  
  // Show confirmation dialog with modern styling
  const confirmed = confirm(
    '🗑️ Delete Comment\n\n' +
    'Are you sure you want to delete this comment?\n\n' +
    '⚠️ This action cannot be undone and will permanently remove the comment and all its replies from the database.\n\n' +
    '❌ All replies to this comment will also be deleted.'
  );
  
  if (!confirmed) {
    return;
  }
  
  const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
  
  // Show loading state on the comment
  const commentElement = document.getElementById(`comment-${commentId}`);
  if (commentElement) {
    commentElement.style.opacity = '0.5';
    commentElement.style.pointerEvents = 'none';
  }
  
  fetch(`/Comment/Delete/${commentId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'RequestVerificationToken': token,
      'X-Requested-With': 'XMLHttpRequest'
    }
  })
  .then(response => {
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  })
  .then(data => {
    if (data.success) {
      // Remove the comment from the page with smooth animation
      if (commentElement) {
        commentElement.style.transition = 'all 0.6s ease';
        commentElement.style.transform = 'translateX(-100%) scale(0.8)';
        commentElement.style.opacity = '0';
        commentElement.style.maxHeight = commentElement.offsetHeight + 'px';
        
        setTimeout(() => {
          commentElement.style.maxHeight = '0';
          commentElement.style.padding = '0';
          commentElement.style.margin = '0';
        }, 200);
        
        setTimeout(() => {
          commentElement.remove();
          showToast('Comment and all replies deleted successfully! 🗑️', 'success');
          
          // Check if there are no more comments
          const remainingComments = document.querySelectorAll('[id^="comment-"]');
          if (remainingComments.length === 0) {
            setTimeout(() => {
              location.reload(); // Reload to show empty state
            }, 1000);
          }
        }, 600);
      } else {
        showToast('Comment deleted successfully! 🗑️', 'success');
      }
    } else {
      // Restore comment state on failure
      if (commentElement) {
        commentElement.style.opacity = '1';
        commentElement.style.pointerEvents = 'auto';
      }
      showToast(data.message || 'Failed to delete comment', 'error');
    }
  })
  .catch(error => {
    console.error('Error deleting comment:', error);
    
    // Restore comment state on error
    if (commentElement) {
      commentElement.style.opacity = '1';
      commentElement.style.pointerEvents = 'auto';
    }
    
    showToast('An error occurred while deleting the comment. Please try again.', 'error');
  })
  .finally(() => {
    // Close any open dropdowns
    document.querySelectorAll('[id^="comment-menu-"]').forEach(menu => {
      menu.classList.add('hidden');
    });
  });
}

// Dropdown toggle function
function toggleDropdown(menuId) {
  const menu = document.getElementById(menuId);
  if (menu) {
    // Close all other dropdowns first
    document.querySelectorAll('[id^="comment-menu-"]').forEach(otherMenu => {
      if (otherMenu.id !== menuId) {
        otherMenu.classList.add('hidden');
      }
    });
    
    // Toggle current dropdown
    menu.classList.toggle('hidden');
  }
}

// Toggle replies visibility
function toggleReplies(commentId) {
  const repliesContainer = document.getElementById(`replies-${commentId}`);
  const chevron = document.getElementById(`chevron-${commentId}`);
  
  if (repliesContainer && chevron) {
    const isHidden = repliesContainer.classList.contains('hidden');
    
    if (isHidden) {
      // Show replies
      repliesContainer.classList.remove('hidden');
      repliesContainer.style.opacity = '0';
      repliesContainer.style.transform = 'translateY(-10px)';
      
      // Animate in
      setTimeout(() => {
        repliesContainer.style.transition = 'all 0.3s ease';
        repliesContainer.style.opacity = '1';
        repliesContainer.style.transform = 'translateY(0)';
      }, 10);
      
      // Rotate chevron
      chevron.style.transform = 'rotate(180deg)';
    } else {
      // Hide replies
      repliesContainer.style.opacity = '0';
      repliesContainer.style.transform = 'translateY(-10px)';
      
      setTimeout(() => {
        repliesContainer.classList.add('hidden');
      }, 300);
      
      // Reset chevron
      chevron.style.transform = 'rotate(0deg)';
    }
  }
}

// Close dropdowns when clicking outside
document.addEventListener('click', function(e) {
  if (!e.target.closest('[onclick*="toggleDropdown"]') && !e.target.closest('[id^="comment-menu-"]')) {
    document.querySelectorAll('[id^="comment-menu-"]').forEach(menu => {
      menu.classList.add('hidden');
    });
  }
});

// Toast notification function
function showToast(message, type = 'info') {
  const toast = document.createElement('div');
  
  let bgColor, borderColor, iconName;  switch (type) {
    case 'success':
      bgColor = 'from-green-600 to-emerald-600';
      borderColor = 'border-green-500';
      iconName = 'check-circle';
      break;
    case 'error':
      bgColor = 'from-red-600 to-red-700';
      borderColor = 'border-red-500';
      iconName = 'x-circle';
      break;
    default:
      bgColor = 'from-green-600 to-emerald-600';
      borderColor = 'border-green-500';
      iconName = 'info';
  }
  
  toast.className = `fixed top-4 right-4 bg-gradient-to-r ${bgColor} text-white px-6 py-4 rounded-xl shadow-2xl z-50 border ${borderColor} max-w-sm transform translate-x-full transition-transform duration-300 flex items-center space-x-3`;
  
  toast.innerHTML = `
    <i data-lucide="${iconName}" class="w-5 h-5 flex-shrink-0"></i>
    <span class="flex-1">${message}</span>
    <button onclick="this.parentElement.remove()" class="text-white/80 hover:text-white transition-colors ml-2">
      <i data-lucide="x" class="w-4 h-4"></i>
    </button>
  `;
  
  document.body.appendChild(toast);
  
  // Initialize icons
  if (typeof lucide !== 'undefined') {
    lucide.createIcons();
  }
  
  // Animate in
  setTimeout(() => {
    toast.style.transform = 'translateX(0)';
  }, 100);
  
  // Auto remove after 5 seconds
  setTimeout(() => {
    if (document.body.contains(toast)) {
      toast.style.transform = 'translateX(100%)';
      setTimeout(() => {
        if (document.body.contains(toast)) {
          toast.remove();
        }
      }, 300);
    }
  }, 5000);
}
