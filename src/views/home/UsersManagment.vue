<template>
  <div class="user-access-management" dir="rtl">
    <div class="page-header">
      <div>
        <h2>مدیریت دسترسی و منطقه کاربران</h2>
        <p>در این صفحه ادمین یا عضو هیأت نظارت استانی می‌تواند نقش و منطقه کاربران مجاز را ویرایش کند.</p>
      </div>
      <button class="btn btn-outline-primary" :disabled="loading" @click="loadInitialData">
        {{ loading ? 'در حال بروزرسانی...' : 'بروزرسانی لیست' }}
      </button>
    </div>

    <div class="filters-card">
      <div class="form-group search-box">
        <label>جستجو</label>
        <input v-model.trim="filters.search" type="text"
          placeholder="کد ملی، کد پرسنلی، نام منطقه یا نقش را جستجو کنید">
      </div>
      <div class="form-group">
        <label>فیلتر نقش</label>
        <select v-model="filters.role">
          <option value="">همه نقش‌ها</option>
          <option v-for="role in roleOptions" :key="role.id" :value="role.name">
            {{ role.name }}
          </option>
        </select>
      </div>
      <div class="form-group">
        <label>فیلتر استان</label>
        <select v-model="filters.provinceCode">
          <option value="">همه استان‌ها</option>
          <option v-for="province in visibleProvinces" :key="province.id" :value="String(province.id)">
            {{ province.name }}
          </option>
        </select>
      </div>
    </div>
    <div v-if="canManageRegionMaxVotes" class="region-votes-card">
      <div class="list-header">
        <div>
          <h3>تنظیم تعداد رأی مجاز مناطق</h3>
          <p>ناظر استان می‌تواند مشخص کند کاربران هر منطقه حداکثر به چند نفر رأی بدهند. </p>
        </div>
        <button class="btn btn-outline-primary" :disabled="loading" @click="loadInitialData">بروزرسانی مناطق</button>
      </div>

      <div class="region-votes-controls">
        <div class="form-group">
          <label>استان</label>
          <select v-model="maxVotesProvinceCode" :disabled="isProvinceSupervisor" @change="syncRegionVoteDefaults">
            <option value="" disabled>انتخاب استان</option>
            <option v-for="province in visibleProvinces" :key="province.id" :value="String(province.id)">
              {{ province.name }}
            </option>
          </select>
        </div>
      </div>
      <div v-if="!maxVotesProvinceAreas.length" class="state-message">برای این استان منطقه‌ای یافت نشد.</div>
      <div v-else class="table-responsive">
        <table class="region-votes-table">
          <thead>
            <tr>
              <th>کد منطقه</th>
              <th>نام منطقه</th>
              <th>واجدین رأی</th>
              <th>تعداد رأی مجاز</th>
              <th>عملیات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="area in maxVotesProvinceAreas" :key="area.id">
              <td>{{ area.id }}</td>
              <td>{{ area.name }}</td>
              <td>{{ voterCountByRegion[area.id] || 0 }}</td>
              <td>
                <input v-model.number="area.maxVotes" class="max-votes-input" type="number" min="1" max="50">
              </td>
              <td>
                <button class="btn btn-sm btn-success" :disabled="regionMaxVotesSaving === String(area.id)"
                  @click="saveAreaMaxVotes(area)">
                  {{ regionMaxVotesSaving === String(area.id) ? 'در حال ذخیره...' : 'ذخیره' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    <div v-if="editMode" class="edit-card">
      <div class="edit-card-header">
        <div>
          <h3>ویرایش کاربر</h3>
          <span>کد ملی: {{ formData.national_id }} | کد پرسنلی: {{ formData.personnel_code || '---' }}</span>
        </div>
        <button type="button" class="btn btn-light" @click="cancelEdit">بستن</button>
      </div>

      <div v-if="assignedRoles.length" class="assigned-roles-block">
        <label>نقش‌های فعلی کاربر</label>
        <ul class="assigned-roles-list">
          <li v-for="role in assignedRoles" :key="role.id">
            <span>{{ role.name }}</span>
            <button type="button" class="btn btn-sm btn-outline-danger" :disabled="revokingRoleId === role.id"
              @click="revokeRole(role)">
              {{ revokingRoleId === role.id ? 'در حال حذف...' : 'لغو دسترسی' }}
            </button>
          </li>
        </ul>
      </div>

      <form class="edit-form" @submit.prevent="saveUser">
        <div class="form-group">
          <label>نقش کاربر</label>
          <select v-model="formData.roleId" required>
            <option v-for="role in roleOptions" :key="role.id" :value="role.id">
              {{ role.name }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>استان</label>
          <select v-model="selectedProvinceCode" required @change="onProvinceChange">
            <option value="" disabled>انتخاب استان</option>
            <option v-for="province in visibleProvinces" :key="province.id" :value="String(province.id)">
              {{ province.name }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>منطقه</label>
          <select v-model="formData.region_id" required :disabled="!selectedProvinceCode">
            <option value="" disabled>انتخاب منطقه</option>
            <option v-for="area in selectedProvinceAreas" :key="area.id" :value="String(area.id)">
              {{ area.name }}
            </option>
          </select>

        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-success" :disabled="saving">
            {{ saving ? 'در حال ذخیره...' : 'ذخیره تغییرات' }}
          </button>
          <button type="button" class="btn btn-outline-secondary" :disabled="saving" @click="cancelEdit">
            انصراف
          </button>
        </div>
      </form>
    </div>

    <div class="user-list-card">
      <div class="list-header">
        <h3>لیست کاربران</h3>
        <span>{{ total }} کاربر (صفحه {{ page }} از {{ totalPages }})</span>
      </div>

      <!-- نوار جستجوی سریع -->
      <div class="search-bar mb-3">
        <input v-model="searchInput" type="text" placeholder="جستجو: کد ملی، نام، کد پرسنلی، منطقه..."
          class="search-input" @input="onSearchInput" />
        <span v-if="searchLoading" class="search-spinner">⏳</span>
        <button v-if="searchInput" class="btn-clear-search" @click="clearSearch">✕</button>
      </div>

      <div v-if="loading" class="state-message">در حال دریافت کاربران از دیتابیس...</div>
      <div v-else-if="!pagedUsers.length" class="state-message">کاربری برای نمایش یافت نشد.</div>

      <div v-else class="table-responsive">
        <table class="users-table">
          <thead>
            <tr>
              <th>کد ملی</th>
              <th>نام</th>
              <th>نام خانوادگی</th>
              <th>کد پرسنلی</th>
              <th>استان</th>
              <th>منطقه</th>
              <th>تحصیلات</th>
              <th>سنوات</th>
              <th>نقش</th>
              <th v-if="currentUserRole === 'ADMIN'">رمز اجرایی</th>
              <th v-if="currentUserRole === 'ADMIN'">رمز نظارت</th>
              <th>تاریخ ایجاد</th>
              <th>عملیات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in pagedUsers" :key="user.id">
              <td>{{ user.national_id }}</td>
              <td>{{ user.first_name }}</td>
              <td>{{ user.last_name }}</td>
              <td>{{ user.personnel_code || '---' }}</td>
              <td>{{ user.provinceName || '---' }}</td>
              <td>{{ user.regionName || '---' }}</td>
              <td>{{ user.education || '---' }}</td>
              <td>{{ user.yearsOfService || '---' }}</td>
              <td><span class="role-badge">{{ getRoleName(user.roles) }}</span></td>
              <td v-if="currentUserRole === 'ADMIN'">
                <div v-if="user.executivePass!==true" class="pass-cell">
                  <span v-if="revealedPasswords.has(user.id + '_exec')" class="box-credential">{{ user.executivePass }}</span>
                  <span v-else class="box-credential masked">●●●●●●●</span>
                  <button class="btn-reveal" @click="togglePass(user.id + '_exec')">
                    {{ revealedPasswords.has(user.id + '_exec') ? 'پنهان' : 'نمایش' }}
                  </button>
                </div>
                <span v-else class="text-muted">---</span>
              </td>
              <td v-if="currentUserRole === 'ADMIN'">
                <div v-if="user.supervisorPass!==true" class="pass-cell">
                  <span v-if="revealedPasswords.has(user.id + '_sup')" class="box-credential box-password">{{ user.supervisorPass }}</span>
                  <span v-else class="box-credential box-password masked">●●●●●●●</span>
                  <button class="btn-reveal" @click="togglePass(user.id + '_sup')">
                    {{ revealedPasswords.has(user.id + '_sup') ? 'پنهان' : 'نمایش' }}
                  </button>
                </div>
                <span v-else class="text-muted">---</span>
              </td>
              <td>{{ user.created_at || '---' }}</td>
              <td>
                <button class="btn btn-sm btn-primary" @click="editUser(user)">ویرایش</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- صفحه‌بندی -->
      <div v-if="totalPages > 1" class="pagination-bar">
        <button class="pg-btn" :disabled="page <= 1" @click="changePage(1)">«</button>
        <button class="pg-btn" :disabled="page <= 1" @click="changePage(page - 1)">‹</button>

        <button v-for="p in pageNumbers" :key="p"
          class="pg-btn" :class="{ active: p === page }"
          @click="changePage(p)">{{ p }}</button>

        <button class="pg-btn" :disabled="page >= totalPages" @click="changePage(page + 1)">›</button>
        <button class="pg-btn" :disabled="page >= totalPages" @click="changePage(totalPages)">»</button>

        <select class="pg-per-page" v-model.number="perPage" @change="onPerPageChange">
          <option :value="20">۲۰ در صفحه</option>
          <option :value="50">۵۰ در صفحه</option>
          <option :value="100">۱۰۰ در صفحه</option>
        </select>
      </div>
    </div>
  </div>
</template>

<script>
import { mapGetters, mapMutations, mapActions } from "vuex";
export default {
  name: 'UsersManagment',
  data() {
    return {
      users: [],
      provinces: [],
      areasByProvince: {},
      loading: false,
      saving: false,
      searchLoading: false,
      editMode: false,
      assignedRoles: [],
      revokingRoleId: '',
      selectedProvinceCode: '',
      maxVotesProvinceCode: '',
      regionMaxVotesSaving: '',
      revealedPasswords: new Set(),
      // صفحه‌بندی سرور
      page: 1,
      perPage: 50,
      total: 0,
      totalPages: 1,
      searchInput: '',
      searchDebounce: null,
      filters: {
        search: '',
        role: '',
        provinceCode: ''
      },
      formData: {
        id: '',
        national_id: '',
        personnel_code: '',
        region_id: '',
        roles: 'VOTER',
        roleId: ''
      },
      roleOptions: [
      ]
    }
  },
  computed: {
    ...mapGetters(["currentUser"]),
    currentUserRole() {
      const roles = this.currentUser?.roles;
      return Array.isArray(roles) ? (roles[0] || '') : (roles || '');
    },
    currentUserRegionId() {
      return String(this.currentUser?.regionId || '');
    },
    isProvinceSupervisor() {
      return this.currentUserRole === 'SUPERVISOR' && this.currentUserRegionId.endsWith('00');
    },canManageRegionMaxVotes() {
      return this.currentUserRole === 'ADMIN' || this.isProvinceSupervisor;
    },
    currentUserProvinceCode() {
      return this.isProvinceSupervisor ? this.currentUserRegionId.slice(0, -2) : '';
    },
    visibleProvinces() {
      if (!this.isProvinceSupervisor) return this.provinces.filter(x=>x.id>1 && x.id%100==0);
      return this.provinces.filter(province => String(province.id) === this.currentUserProvinceCode);
    },
    selectedProvinceAreas() {
      return this.areasByProvince[this.selectedProvinceCode/100] || [];
    }, maxVotesProvinceAreas() {
      return this.areasByProvince[this.maxVotesProvinceCode/100] || [];
    },
    voterCountByRegion() {
      const map = {};
      this.users.forEach(u => {
        if (u.roles === 'VOTER') {
          const rid = Number(u.region_id);
          map[rid] = (map[rid] || 0) + 1;
        }
      });
      return map;
    },
    filteredUsers() {
      return this.users.filter(user => {
        const matchesRole     = !this.filters.role         || user.roles === this.filters.role;
        const matchesProvince = !this.filters.provinceCode || String(user.provinceCode) === this.filters.provinceCode;
        return matchesRole && matchesProvince;
      });
    },
    pagedUsers() {
      return this.filteredUsers;
    },
    pageNumbers() {
      const pages = this.totalPages;
      const cur   = this.page;
      const delta = 2;
      const range = [];
      for (let i = Math.max(1, cur - delta); i <= Math.min(pages, cur + delta); i++) {
        range.push(i);
      }
      return range;
    }
  },
  mounted() {
    this.loadInitialData();
  },
  methods: {
    ...mapMutations(["setsidebarVisible"]),
    ...mapActions(["getUsers", "updateUser", "getRegions","saveRegionMaxVotes","Getroles","assignUserRoles"]),
    async loadInitialData() {
      this.loading = true;
      try {
        const [usersRes, regionsResponse,roles] = await Promise.all([
          this.getUsers({ page: this.page, limit: this.perPage, search: this.searchInput }),
          this.getRegions(),
          this.Getroles()
        ]);

        this.users      = Array.isArray(usersRes?.data) ? usersRes.data : [];
        this.total      = usersRes?.meta?.total ?? this.users.length;
        this.totalPages = usersRes?.meta?.pages  ?? 1;

        this.provinces       = regionsResponse?.data || [];
        this.roleOptions       = roles?.data || [];
        this.areasByProvince = regionsResponse?.areasByProvince || {};

        if (this.isProvinceSupervisor) {
          this.filters.provinceCode = this.currentUserProvinceCode;
          this.maxVotesProvinceCode = this.currentUserProvinceCode;
        } else if (!this.maxVotesProvinceCode && this.visibleProvinces.length) {
          this.maxVotesProvinceCode = String(this.visibleProvinces[0].id);
        }
        this.syncRegionVoteDefaults();
      } catch (error) {
        this.$bvToast.toast("خطا در بارگذاری کاربران", { title: "خطا", variant: "danger", solid: true });
      } finally {
        this.loading = false;
      }
    },
    async fetchUsers() {
      this.loading = true;
      try {
      //   const payload = {
      //   keyword: this.searchInput,
      //   pageNumber: this.page,
      //   pageSize: this.perPage,
      //   isActive: true
      // };
      // const res       = await this.getUsers(payload);
        const res       = await this.getUsers({ page: this.page, limit: this.perPage, search: this.searchInput });
        this.users      = Array.isArray(res?.data) ? res.data : [];
        this.total      = res?.meta?.total ?? this.users.length;
        this.totalPages = res?.meta?.pages  ?? 1;
      } finally {
        this.loading       = false;
        this.searchLoading = false;
      }
    },
    changePage(p) {
      if (p < 1 || p > this.totalPages) return;
      this.page = p;
      this.fetchUsers();
    },
    onPerPageChange() {
      this.page = 1;
      this.fetchUsers();
    },
    onSearchInput() {
      this.searchLoading = true;
      clearTimeout(this.searchDebounce);
      this.searchDebounce = setTimeout(() => {
        this.page = 1;
        this.fetchUsers();
      }, 1000);
    },
    clearSearch() {
      this.searchInput = '';
      this.page = 1;
      this.fetchUsers();
    },
    syncRegionVoteDefaults() {
      this.maxVotesProvinceAreas.forEach(area => {
        const parsedMaxVotes = Number(area.maxVotes);
        if (!parsedMaxVotes || parsedMaxVotes < 1) {
          this.$set(area, 'maxVotes', 1);
        }
      });
    },
    async saveAreaMaxVotes(area) {
      const maxVotes = Number(area.maxVotes);
      if (!area?.id || !Number.isInteger(maxVotes) || maxVotes < 1 || maxVotes > 50) {
        this.showToast('تعداد رأی مجاز باید عددی بین ۱ تا ۵۰ باشد.', 'warning');
        return;
      }
      if (this.isProvinceSupervisor && String(this.maxVotesProvinceCode) !== this.currentUserProvinceCode) {
        this.showToast('اعضای هیأت نظارت استانی فقط مجاز به ویرایش مناطق استان خود هستند.', 'warning');
        return;
      }

      this.regionMaxVotesSaving = String(area.id);
      try {
        const response = await this.saveRegionMaxVotes({
          electionCycleId:1,
          regionId: area.id,
          region_id: area.id,
          maxVotes
        });
        if (!response?.succeeded) {
          throw new Error('Save max votes failed');
        }
        this.$set(area, 'maxVotes', maxVotes);
        this.showToast('تعداد رأی مجاز منطقه ذخیره شد.', 'success');
      } catch (error) {
        
        this.showToast('ذخیره تعداد رأی مجاز انجام نشد. دوباره تلاش کنید.', 'danger');
      } finally {
        this.regionMaxVotesSaving = '';
      }
    },
    editUser(user) {
      const currentRoleName = Array.isArray(user.roles) ? user.roles[0] : user.roles;
      const matchedRole = this.roleOptions.find(
        role => String(role.name).toUpperCase() === String(currentRoleName || '').toUpperCase()
      );
      this.formData = {
        id: user.id,
        national_id: user.national_id,
        personnel_code: user.personnel_code,
        region_id: user.region_id ? String(user.region_id) : '',
        roles: user.roles || 'VOTER',
        roleId: matchedRole ? matchedRole.id : ''
      };
      this.assignedRoles = this.getAssignedRoles(user.roles);
      this.selectedProvinceCode = user.provinceCode ? String(user.provinceCode) : this.findProvinceCodeByRegion(user.region_id);
      this.editMode = true;
      this.$nextTick(() => window.scrollTo({ top: 0, behavior: 'smooth' }));
    },
    getAssignedRoles(rolesValue) {
      let roleNames = [];
      if (Array.isArray(rolesValue)) {
        roleNames = rolesValue.map(r => String(r));
      } else if (typeof rolesValue === 'string') {
        roleNames = rolesValue.split(',').map(r => r.trim()).filter(Boolean);
      }
      return roleNames
        .map(name => this.roleOptions.find(role => String(role.name).toUpperCase() === name.toUpperCase()))
        .filter(Boolean);
    },
    async revokeRole(role) {
      if (!this.formData.id || !role?.id) return;
      this.revokingRoleId = role.id;
      try {
        const response = await this.assignUserRoles({
          id: this.formData.id,
          userRoles: [
            {
              roleId: role.id,
              roleName: role.name,
              description: role.description || '',
              enabled: false
            }
          ]
        });
        if (response === false || response?.succeeded === false) {
          throw new Error('Revoke failed');
        }
        this.assignedRoles = this.assignedRoles.filter(r => r.id !== role.id);
        if (this.formData.roleId === role.id) {
          this.formData.roleId = '';
        }
        this.showToast('دسترسی نقش لغو شد.', 'success');
      } catch (error) {
        this.showToast('لغو دسترسی نقش انجام نشد. دوباره تلاش کنید.', 'danger');
      } finally {
        this.revokingRoleId = '';
      }
    },
    onProvinceChange() {
      const areas = this.selectedProvinceAreas;
      this.formData.region_id = areas.length ? String(areas[0].id) : '';
    },
    findProvinceCodeByRegion(regionId) {
      const region = String(regionId || '');
      const province = this.visibleProvinces.find(item => region.startsWith(String(item.id)));
      return province ? String(province.id) : '';
    },
    async saveUser() {
      if (!this.formData.national_id || !this.formData.region_id || !this.formData.roleId) {
        this.showToast('لطفاً نقش و منطقه کاربر را کامل انتخاب کنید.', 'warning');
        return;
      }
      if (this.isProvinceSupervisor && String(this.selectedProvinceCode) !== this.currentUserProvinceCode) {
        this.showToast('اعضای هیأت نظارت استانی فقط مجاز به ویرایش کاربران استان خود هستند.', 'warning');
        return;
      }

      const selectedRole = this.roleOptions.find(role => role.id === this.formData.roleId);
      if (!selectedRole) {
        this.showToast('نقش انتخاب‌شده معتبر نیست.', 'warning');
        return;
      }

      this.saving = true;
      try {
        const [updateResponse, rolesResponse] = await Promise.all([
          this.updateUser({
            national_id: this.formData.national_id,
            region_id: this.formData.region_id
          }),
          this.assignUserRoles({
            id: this.formData.id,
            userRoles: [
              {
                roleId: selectedRole.id,
                roleName: selectedRole.name,
                description: selectedRole.description || '',
                enabled: true
              }
            ]
          })
        ]);

        if (updateResponse === false || rolesResponse === false || rolesResponse?.succeeded === false) {
          throw new Error('Update failed');
        }
        this.cancelEdit();
      } catch (error) {
        this.showToast('ذخیره تغییرات انجام نشد. دوباره تلاش کنید.', 'danger');
      } finally {
        this.saving = false;
      }
    },
    cancelEdit() {
      this.editMode = false;
      this.selectedProvinceCode = '';
      this.assignedRoles = [];
      this.revokingRoleId = '';
      this.formData = {
        id: '',
        national_id: '',
        personnel_code: '',
        region_id: '',
        roles: 'VOTER',
        roleId: ''
      };
    },
   getRoleName(role) {

  const roles = {
    EXECUTIVE: 'کاربر اجرایی',
    SUPERVISOR: 'کاربر نظارت',
    VOTER: 'کاربر عادی',
    CANDIDATE: 'کاندید',
    ADMIN: 'کاربر ادمین'
  };

  if (!role) return '---';

  // اگر آرایه باشد
  if (Array.isArray(role)) {
    return role
      .map(r => roles[String(r).toUpperCase()] || r)
      .join('، ');
  }

  // اگر رشته با کاما باشد
  if (typeof role === 'string' && role.includes(',')) {
    return role
      .split(',')
      .map(r => r.trim())
      .map(r => roles[r.toUpperCase()] || r)
      .join('، ');
  }

  // یک رول
  return roles[String(role).toUpperCase()] || role;
},
    togglePass(key) {
      const s = new Set(this.revealedPasswords);
      s.has(key) ? s.delete(key) : s.add(key);
      this.revealedPasswords = s;
    },
    showToast(message, variant) {
      if (this.$bvToast) {
        this.$bvToast.toast(message, {
          title: variant === 'success' ? 'موفق' : 'پیام سیستم',
          variant,
          solid: true
        });
      } else {
        alert(message);
      }
    }
  }
}
</script>

<style scoped>
.user-access-management {
  max-width: 1280px;
  margin: 0 auto;
  padding: 24px;
}

.page-header,
.filters-card,
.region-votes-card,
.edit-card,
.user-list-card {
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.07);
  padding: 20px;
  margin-bottom: 20px;
}

.page-header,
.edit-card-header,
.list-header,
.form-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.page-header h2,
.edit-card h3,
.region-votes-card h3,
.user-list-card h3 {
  margin: 0 0 8px;
  font-weight: 700;
  color: #25324b;
}

.page-header p,
.edit-card-header span,
.list-header span {
  margin: 0;
  color: #6c757d;
}

.filters-card,
.edit-form {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 16px;
}

.assigned-roles-block {
  margin-bottom: 16px;
}

.assigned-roles-block label {
  display: block;
  font-weight: 600;
  color: #495057;
  margin-bottom: 8px;
}

.assigned-roles-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.assigned-roles-list li {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #f8f9fb;
  border: 1px solid #eef1f5;
  border-radius: 999px;
  padding: 6px 8px 6px 14px;
}

.btn-outline-danger {
  background: #fff;
  border: 1px solid #f1b0b7;
  color: #dc3545;
}

.btn-outline-danger:hover:not(:disabled) {
  background: #fbeaec;
}

.btn-outline-danger:disabled {
  opacity: 0.6;
  cursor: default;
}
.region-votes-controls {
  display: grid;
  grid-template-columns: minmax(220px, 320px);
  gap: 16px;
  margin-bottom: 16px;
}

.region-votes-card p {
  margin: 0;
  color: #6c757d;
}

.region-votes-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 620px;
}

.region-votes-table th,
.region-votes-table td {
  padding: 12px;
  border-bottom: 1px solid #eef1f5;
  text-align: right;
}

.max-votes-input {
  width: 110px;
  min-height: 38px;
  border: 1px solid #d9dee7;
  border-radius: 10px;
  padding: 6px 10px;
}
.search-box {
  grid-column: span 2;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-weight: 600;
  color: #495057;
}

button {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s;
}


.form-group input,
.form-group select {
  min-height: 42px;
  border: 1px solid #d9dee7;
  border-radius: 10px;
  padding: 8px 12px;
  background: #fff;
}

.form-actions {
  justify-content: flex-start;
  align-self: end;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 900px;
}

.users-table th,
.users-table td {
  padding: 12px 10px;
  border-bottom: 1px solid #eef1f5;
  text-align: right;
  vertical-align: middle;
}

.users-table th {
  background: #f8f9fb;
  color: #495057;
  font-weight: 700;
}

.role-badge {
  display: inline-flex;
  padding: 6px 10px;
  border-radius: 999px;
  background: #edf6ff;
  color: #1464a5;
  font-weight: 600;
  white-space: nowrap;
}

.state-message {
  padding: 28px;
  text-align: center;
  color: #6c757d;
  background: #f8f9fb;
  border-radius: 12px;
}

.pass-cell {
  display: flex;
  align-items: center;
  gap: 6px;
}

.box-credential {
  display: inline-block;
  font-family: monospace;
  font-size: 13px;
  background: #f0f4ff;
  color: #1a3c7a;
  border: 1px solid #c5d5f0;
  border-radius: 6px;
  padding: 3px 8px;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.box-credential.masked {
  color: #94a3b8;
  letter-spacing: 2px;
}

.box-password {
  background: #fff7ed;
  color: #92400e;
  border-color: #f0c97a;
}

.btn-reveal {
  padding: 2px 8px;
  font-size: 11px;
  border: 1px solid #c5d5f0;
  border-radius: 5px;
  background: #fff;
  color: #1a3c7a;
  cursor: pointer;
  white-space: nowrap;
  transition: background 0.15s;
}

.btn-reveal:hover {
  background: #e8f0fe;
}

.text-muted {
  color: #adb5bd;
}

/* جستجو */
.search-bar {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input {
  width: 100%;
  height: 42px;
  border: 1px solid #d9dee7;
  border-radius: 10px;
  padding: 8px 40px 8px 36px;
  font-size: 0.9rem;
}

.search-input:focus {
  outline: none;
  border-color: #3f51b5;
  box-shadow: 0 0 0 3px rgba(63,81,181,0.1);
}

.search-spinner {
  position: absolute;
  left: 36px;
  font-size: 0.85rem;
}

.btn-clear-search {
  position: absolute;
  left: 10px;
  background: none;
  border: none;
  color: #94a3b8;
  font-size: 1rem;
  cursor: pointer;
  padding: 0 4px;
  line-height: 1;
}

.btn-clear-search:hover { color: #ef4444; }

/* صفحه‌بندی */
.pagination-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 20px;
  padding-top: 16px;
  border-top: 1px solid #eef1f5;
}

.pg-btn {
  min-width: 36px;
  height: 36px;
  padding: 0 10px;
  border: 1px solid #d9dee7;
  border-radius: 8px;
  background: #fff;
  color: #334155;
  font-size: 0.88rem;
  cursor: pointer;
  transition: all 0.15s;
}

.pg-btn:hover:not(:disabled) {
  background: #f0f2ff;
  border-color: #3f51b5;
  color: #3f51b5;
}

.pg-btn.active {
  background: #3f51b5;
  border-color: #3f51b5;
  color: #fff;
  font-weight: 700;
}

.pg-btn:disabled {
  opacity: 0.4;
  cursor: default;
}

.pg-per-page {
  height: 36px;
  border: 1px solid #d9dee7;
  border-radius: 8px;
  padding: 0 10px;
  font-size: 0.82rem;
  color: #475569;
  cursor: pointer;
  margin-right: 8px;
}

@media (max-width: 768px) {
  .user-access-management {
    padding: 12px;
  }

  .page-header,
  .edit-card-header,
  .list-header,
  .form-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .search-box {
    grid-column: span 1;
  }
}
</style>