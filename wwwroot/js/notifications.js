// Notification System JavaScript
(function() {
    'use strict';

    // Notification state
    let notificationsLoaded = false;

    // Helper function to get notification icon and color based on type
    function getNotificationStyle(type) {
        const styles = {
            0: { icon: 'ti-info-circle', color: 'info' },      // Info
            1: { icon: 'ti-circle-check', color: 'success' },  // Success
            2: { icon: 'ti-alert-triangle', color: 'warning' }, // Warning
            3: { icon: 'ti-alert-circle', color: 'danger' },    // Error
            4: { icon: 'ti-file-invoice', color: 'primary' },   // Invoice
            5: { icon: 'ti-camera', color: 'purple' },          // PhotoShoot
            6: { icon: 'ti-users', color: 'teal' },             // Client
            7: { icon: 'ti-photo', color: 'indigo' }            // Album
        };
        return styles[type] || styles[0];
    }

    // Helper function to format time ago
    function timeAgo(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now - date) / 1000);

        const intervals = {
            year: 31536000,
            month: 2592000,
            week: 604800,
            day: 86400,
            hour: 3600,
            minute: 60
        };

        for (const [unit, secondsInUnit] of Object.entries(intervals)) {
            const interval = Math.floor(seconds / secondsInUnit);
            if (interval >= 1) {
                return interval === 1 ? `1 ${unit} ago` : `${interval} ${unit}s ago`;
            }
        }
        return 'Just now';
    }

    // Load notifications
    async function loadNotifications() {
        try {
            const response = await fetch('/api/notifications?limit=10');
            const notifications = await response.json();

            renderNotifications(notifications);
            notificationsLoaded = true;
        } catch (error) {
            console.error('Error loading notifications:', error);
            document.getElementById('notificationsList').innerHTML = `
                <div class="text-center py-4 text-danger">
                    <i class="ti ti-alert-circle fs-3"></i>
                    <p class="mb-0 mt-2">Failed to load notifications</p>
                </div>
            `;
        }
    }

    // Render notifications
    function renderNotifications(notifications) {
        const container = document.getElementById('notificationsList');

        if (!notifications || notifications.length === 0) {
            container.innerHTML = `
                <div class="text-center py-5">
                    <i class="ti ti-bell-off fs-1 text-muted"></i>
                    <p class="text-muted mt-2 mb-0">No notifications</p>
                </div>
            `;
            return;
        }

        let html = '';
        notifications.forEach(notification => {
            const style = getNotificationStyle(notification.type);
            const isRead = notification.isRead;
            const bgClass = isRead ? '' : 'bg-light';

            html += `
                <div class="notification-item ${bgClass} border-bottom border-light p-3" data-notification-id="${notification.id}" data-link="${notification.link || '#'}">
                    <div class="d-flex align-items-start">
                        <div class="flex-shrink-0">
                            <div class="avatar avatar-sm bg-${style.color}-subtle">
                                <span class="avatar-title text-${style.color} rounded-circle">
                                    <i class="ti ${style.icon} fs-lg"></i>
                                </span>
                            </div>
                        </div>
                        <div class="flex-grow-1 ms-3">
                            ${notification.title ? `<h6 class="mb-1 fw-semibold">${notification.title}</h6>` : ''}
                            <p class="mb-1 text-muted fs-sm">${notification.message}</p>
                            <small class="text-muted">
                                <i class="ti ti-clock me-1"></i>${timeAgo(notification.createdDate)}
                            </small>
                        </div>
                        <div class="flex-shrink-0 ms-2">
                            ${!isRead ? '<span class="badge bg-primary rounded-circle p-1" style="width: 8px; height: 8px;"></span>' : ''}
                        </div>
                    </div>
                </div>
            `;
        });

        container.innerHTML = html;

        // Add click handlers
        document.querySelectorAll('.notification-item').forEach(item => {
            item.style.cursor = 'pointer';
            item.addEventListener('click', function() {
                const id = parseInt(this.dataset.notificationId);
                const link = this.dataset.link;
                markAsRead(id, link);
            });
        });
    }

    // Mark notification as read
    async function markAsRead(id, link) {
        try {
            await fetch(`/api/notifications/${id}/read`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            // Reload notifications and update count
            await Promise.all([loadNotifications(), updateUnreadCount()]);

            // Navigate to link if provided
            if (link && link !== '#') {
                window.location.href = link;
            }
        } catch (error) {
            console.error('Error marking notification as read:', error);
        }
    }

    // Mark all as read
    async function markAllAsRead() {
        try {
            await fetch('/api/notifications/mark-all-read', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            // Reload notifications and update count
            await Promise.all([loadNotifications(), updateUnreadCount()]);
        } catch (error) {
            console.error('Error marking all as read:', error);
        }
    }

    // Update unread count badge
    async function updateUnreadCount() {
        try {
            const response = await fetch('/api/notifications/unread-count');
            const data = await response.json();
            const badge = document.getElementById('notificationBadge');
            const countText = document.getElementById('notificationCount');

            if (data.count > 0) {
                badge.textContent = data.count > 99 ? '99+' : data.count;
                badge.style.display = 'inline-block';
                if (countText) {
                    countText.textContent = data.count > 99 ? '99+' : data.count;
                }
            } else {
                badge.style.display = 'none';
                if (countText) {
                    countText.textContent = '0';
                }
            }
        } catch (error) {
            console.error('Error updating unread count:', error);
        }
    }

    // Initialize when DOM is ready
    document.addEventListener('DOMContentLoaded', function() {
        // Update unread count on page load
        updateUnreadCount();

        // Load notifications when dropdown is opened
        const notificationDropdown = document.querySelector('[data-bs-toggle="dropdown"]');
        if (notificationDropdown) {
            notificationDropdown.addEventListener('click', function() {
                if (!notificationsLoaded) {
                    loadNotifications();
                }
            });
        }

        // Mark all as read button
        const markAllReadBtn = document.getElementById('markAllRead');
        if (markAllReadBtn) {
            markAllReadBtn.addEventListener('click', function(e) {
                e.preventDefault();
                markAllAsRead();
            });
        }

        // Refresh notifications every 2 minutes
        setInterval(() => {
            updateUnreadCount();
            if (notificationsLoaded) {
                loadNotifications();
            }
        }, 120000); // 2 minutes
    });

    // Expose functions globally if needed
    window.NotificationSystem = {
        loadNotifications,
        updateUnreadCount,
        markAsRead,
        markAllAsRead
    };
})();
