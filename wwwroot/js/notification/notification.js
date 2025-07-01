document.addEventListener("DOMContentLoaded", function () {
  lucide.createIcons();
});

function markAsRead(notificationId) {
  fetch(`/Notification/MarkAsRead`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'meta[name="__RequestVerificationToken"]'
      ).content,
    },
    body: JSON.stringify({ id: notificationId }),
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        const item = document.querySelector(`[data-id="${notificationId}"]`);
        item.classList.remove("bg-accent/5", "border-l-4", "border-accent");
        item.querySelector(".inline-flex.items-center.px-2.py-1")?.remove();        item.querySelector('button[onclick*="markAsRead"]')?.remove();
      }
    })
    .catch((error) => console.error("Hata:", error));
}

function markAllAsRead() {
  if (!confirm("Tüm bildirimleri okundu olarak işaretlemek istiyor musunuz?")) return;
  fetch(`/Notification/MarkAllAsRead`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'meta[name="__RequestVerificationToken"]'
      ).content,
    },
  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        location.reload();
      }
    })
    .catch((error) => console.error("Hata:", error));
}

function deleteNotification(notificationId) {
  if (!confirm("Bu bildirimi silmek istiyor musunuz?")) return;

  fetch(`/Notification/Delete`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'meta[name="__RequestVerificationToken"]'
      ).content,
    },
    body: JSON.stringify({ id: notificationId }),
  })
    .then((response) => response.json())    .then((data) => {
      if (data.success) {
        document.querySelector(`[data-id="${notificationId}"]`).remove();
      }
    })
    .catch((error) => console.error("Hata:", error));
}

function clearAllNotifications() {  if (!confirm("Tüm bildirimleri silmek istiyor musunuz? Bu işlem geri alınamaz."))
    return;

  fetch(`/Notification/DeleteAll`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      RequestVerificationToken: document.querySelector(
        'meta[name="__RequestVerificationToken"]'
      ).content,
    },  })
    .then((response) => response.json())
    .then((data) => {
      if (data.success) {
        location.reload();
      }
    })
    .catch((error) => console.error("Hata:", error));
}
