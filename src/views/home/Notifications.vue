<template>
  <div class="notif-page">

    <!-- هدر -->
    <div class="notif-header mb-4">
      <div class="d-flex align-items-center gap-2">
        <div class="header-icon">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none">
            <path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z" fill="currentColor"/>
          </svg>
        </div>
        <div>
          <h5 class="mb-0 fw-bold">اطلاعیه‌ها</h5>
          <small class="text-muted">{{ announcements.length }} اطلاعیه</small>
        </div>
      </div>
      <b-badge v-if="newCount > 0" pill variant="danger" class="new-count-badge">
        {{ newCount }} جدید
      </b-badge>
    </div>

    <!-- لودینگ -->
    <div v-if="loading" class="text-center py-5">
      <b-spinner variant="primary" style="width:2.5rem;height:2.5rem;" />
      <p class="mt-3 text-muted">در حال بارگذاری...</p>
    </div>

    <!-- خالی -->
    <div v-else-if="!announcements.length" class="empty-state">
      <div class="empty-icon">
        <svg width="56" height="56" viewBox="0 0 24 24" fill="none">
          <path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z" fill="currentColor" opacity="0.3"/>
        </svg>
      </div>
      <p class="text-muted mt-2">اطلاعیه‌ای موجود نیست</p>
    </div>

    <!-- لیست اطلاعیه‌ها -->
    <div v-else class="notif-list">
      <div
        v-for="item in announcements"
        :key="item.id"
        class="notif-item"
        :class="{ 'notif-item--new': isNew(item.id) }"
        @click="openNotif(item)"
      >
        <!-- نوار رنگی سمت راست -->
        <div class="notif-accent" :class="`accent--${item.target_scope}`"></div>

        <!-- آیکون -->
        <div class="notif-icon-wrap" :class="`icon-wrap--${item.target_scope}`">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
            <path d="M12 22C13.1 22 14 21.1 14 20H10C10 21.1 10.9 22 12 22ZM18 16V11C18 7.93 16.36 5.36 13.5 4.68V4C13.5 3.17 12.83 2.5 12 2.5C11.17 2.5 10.5 3.17 10.5 4V4.68C7.63 5.36 6 7.92 6 11V16L4 18V19H20V18L18 16Z" fill="currentColor"/>
          </svg>
        </div>

        <!-- محتوا -->
        <div class="notif-body">
          <div class="d-flex align-items-center gap-2 flex-wrap mb-1">
            <span class="notif-title">{{ item.title }}</span>
            <span v-if="isNew(item.id)" class="new-dot"></span>
          </div>
          <div class="d-flex align-items-center gap-2">
            <span class="scope-pill" :class="`scope-pill--${item.target_scope}`">
              {{ scopeLabel(item.target_scope) }}
            </span>
            <span class="notif-date">{{ item.created_at_shamsi }}</span>
          </div>
        </div>

        <!-- فلش -->
        <div class="notif-arrow">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none">
            <path d="M15 18L9 12L15 6" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
      </div>
    </div>

    <!-- مودال جزئیات -->
    <b-modal
      v-model="showNotif"
      hide-header
      hide-footer
      size="lg"
      centered
      body-class="p-0"
      @hidden="clearRoute"
    >
      <div v-if="selectedNotif" class="notif-modal">
        <!-- هدر مودال -->
        <div class="modal-header-custom" :class="`modal-header--${selectedNotif.target_scope}`">
          <div class="d-flex align-items-start justify-content-between">
            <div class="flex-grow-1">
              <span class="scope-pill scope-pill--light mb-2 d-inline-block">
                {{ scopeLabel(selectedNotif.target_scope) }}
              </span>
              <h5 class="modal-title-custom mb-0">{{ selectedNotif.title }}</h5>
            </div>
            <button class="modal-close-btn" @click="showNotif = false">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none">
                <path d="M18 6L6 18M6 6L18 18" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
              </svg>
            </button>
          </div>
          <small class="modal-date">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" style="margin-left:4px">
              <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2"/>
              <path d="M12 6V12L16 14" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
            </svg>
            {{ selectedNotif.created_at_full }}
          </small>
        </div>

        <!-- متن -->
        <div class="modal-body-custom">
          <div class="notif-content">{{ selectedNotif.content }}</div>
        </div>

        <!-- فوتر -->
        <div class="modal-footer-custom">
          <b-button variant="primary" size="sm" @click="showNotif = false">بستن</b-button>
        </div>
      </div>
    </b-modal>

  </div>
</template>

<script>
import { mapGetters, mapActions } from "vuex";

export default {
  name: "NotificationsPage",
  computed: {
    ...mapGetters(["announcements", "currentUser"]),
    newCount() {
      return this.announcements.filter(n => this.isNew(n.id)).length;
    },
  },
  data() {
    return {
      loading: false,
      showNotif: false,
      selectedNotif: null,
      seenIds: JSON.parse(localStorage.getItem("seenAnnouncements") || "[]"),
    };
  },
  methods: {
    ...mapActions(["getAnnouncements"]),
    isNew(id)    { return !this.seenIds.includes(id); },
    markSeen(id) {
      if (!this.seenIds.includes(id)) {
        this.seenIds.push(id);
        localStorage.setItem("seenAnnouncements", JSON.stringify(this.seenIds));
      }
    },
    scopeLabel(s) { return { country: "سراسری", province: "استانی", region: "منطقه‌ای" }[s] || s; },
    clearRoute()  { this.$router.replace({ query: {} }); },
    openNotif(item) {
      this.selectedNotif = item;
      this.showNotif = true;
      this.markSeen(item.id);
      this.$router.replace({ query: { id: item.id } });
    },
    checkRouteForModal() {
      const id = Number(this.$route.query.id);
      if (!id) return;
      const notif = this.announcements.find(n => n.id === id);
      if (notif) { this.selectedNotif = notif; this.showNotif = true; this.markSeen(id); }
    },
  },
  async mounted() {
    this.loading = true;
    await this.getAnnouncements();
    this.loading = false;
    this.checkRouteForModal();
  },
};
</script>

<style scoped>
/* ── صفحه ── */
.notif-page { max-width: 680px; margin: 0 auto; padding: 8px 0; }

/* ── هدر ── */
.notif-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.header-icon {
  width: 40px; height: 40px;
  background: linear-gradient(135deg, #3f51b5, #5c6bc0);
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  color: white;
}
.new-count-badge { font-size: 0.8rem; padding: 6px 12px; }

/* ── خالی ── */
.empty-state { text-align: center; padding: 60px 0; color: #94a3b8; }
.empty-icon  { color: #3f51b5; }

/* ── آیتم ── */
.notif-list   { display: flex; flex-direction: column; gap: 10px; }
.notif-item   {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #fff;
  border-radius: 16px;
  border: 1.5px solid #e8eef5;
  padding: 16px 14px;
  cursor: pointer;
  transition: all 0.18s ease;
  position: relative;
  overflow: hidden;
}
.notif-item:hover {
  border-color: #3f51b5;
  box-shadow: 0 4px 16px rgba(63,81,181,0.10);
  transform: translateY(-1px);
}
.notif-item--new {
  background: #f5f7ff;
  border-color: #c5cae9;
}

/* نوار رنگی */
.notif-accent {
  position: absolute;
  right: 0; top: 0; bottom: 0;
  width: 4px;
  border-radius: 0 16px 16px 0;
}
.accent--country  { background: #ef4444; }
.accent--province { background: #f59e0b; }
.accent--region   { background: #3b82f6; }

/* آیکون */
.notif-icon-wrap {
  width: 42px; height: 42px; min-width: 42px;
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.icon-wrap--country  { background: #fee2e2; color: #ef4444; }
.icon-wrap--province { background: #fef3c7; color: #d97706; }
.icon-wrap--region   { background: #dbeafe; color: #3b82f6; }

/* بدنه */
.notif-body  { flex: 1; min-width: 0; }
.notif-title { font-size: 0.92rem; font-weight: 600; color: #1e293b; line-height: 1.4; }
.notif-date  { font-size: 0.72rem; color: #94a3b8; }
.new-dot     {
  width: 8px; height: 8px; min-width: 8px;
  background: #ef4444;
  border-radius: 50%;
  display: inline-block;
}

/* scope pill */
.scope-pill {
  display: inline-block;
  font-size: 0.68rem;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 20px;
  letter-spacing: 0.3px;
}
.scope-pill--country  { background: #fee2e2; color: #dc2626; }
.scope-pill--province { background: #fef3c7; color: #b45309; }
.scope-pill--region   { background: #dbeafe; color: #1d4ed8; }
.scope-pill--light    { background: rgba(255,255,255,0.25); color: white; }

/* فلش */
.notif-arrow { color: #cbd5e1; flex-shrink: 0; }

/* ── مودال ── */
.notif-modal { border-radius: 16px; overflow: hidden; }

.modal-header-custom {
  padding: 24px 24px 16px;
  color: white;
}
.modal-header--country  { background: linear-gradient(135deg, #ef4444, #dc2626); }
.modal-header--province { background: linear-gradient(135deg, #f59e0b, #d97706); }
.modal-header--region   { background: linear-gradient(135deg, #3f51b5, #5c6bc0); }

.modal-title-custom { font-size: 1.1rem; font-weight: 700; line-height: 1.5; }
.modal-date { font-size: 0.75rem; opacity: 0.85; margin-top: 6px; display: block; }

.modal-close-btn {
  background: rgba(255,255,255,0.2);
  border: none;
  border-radius: 8px;
  padding: 4px;
  color: white;
  cursor: pointer;
  flex-shrink: 0;
  margin-right: 12px;
  transition: background 0.15s;
}
.modal-close-btn:hover { background: rgba(255,255,255,0.35); }

.modal-body-custom {
  padding: 24px;
  min-height: 120px;
}
.notif-content {
  font-size: 0.95rem;
  line-height: 2;
  color: #334155;
  white-space: pre-wrap;
}

.modal-footer-custom {
  padding: 12px 24px;
  border-top: 1px solid #f1f5f9;
  display: flex;
  justify-content: flex-start;
}
</style>
