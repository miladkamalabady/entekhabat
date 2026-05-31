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
          <option v-for="role in roleOptions" :key="role.value" :value="role.value">
            {{ role.text }}
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

    <div v-if="editMode" class="edit-card">
      <div class="edit-card-header">
        <div>
          <h3>ویرایش کاربر</h3>
          <span>کد ملی: {{ formData.national_id }} | کد پرسنلی: {{ formData.personnel_code || '---' }}</span>
        </div>
        <button type="button" class="btn btn-light" @click="cancelEdit">بستن</button>
      </div>

      <form class="edit-form" @submit.prevent="saveUser">
        <div class="form-group">
          <label>نقش کاربر</label>
          <select v-model="formData.roles" required>
            <option v-for="role in roleOptions" :key="role.value" :value="role.value">
              {{ role.text }}
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
        <span>{{ filteredUsers.length }} کاربر</span>
      </div>

      <div v-if="loading" class="state-message">در حال دریافت کاربران از دیتابیس...</div>
      <div v-else-if="!filteredUsers.length" class="state-message">کاربری برای نمایش یافت نشد.</div>

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
              <th>تاریخ ایجاد</th>
              <th>عملیات</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id">
              <td>{{ user.national_id }}</td>
              <td>{{ user.first_name }}</td>
              <td>{{ user.last_name }}</td>
              <td>{{ user.personnel_code || '---' }}</td>
              <td>{{ user.provinceName || '---' }}</td>
              <td>{{ user.regionName || '---' }}</td>
              <td>{{ user.education || '---' }}</td>
              <td>{{ user.yearsOfService || '---' }}</td>
              <td><span class="role-badge">{{ getRoleName(user.roles) }}</span></td>
              <td>{{ user.created_at || '---' }}</td>
              <td>
                <button class="btn btn-sm btn-primary" @click="editUser(user)">ویرایش</button>
              </td>
            </tr>
          </tbody>
        </table>
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
      editMode: false,
      selectedProvinceCode: '',
      filters: {
        search: '',
        role: '',
        provinceCode: ''
      },
      formData: {
        national_id: '',
        personnel_code: '',
        region_id: '',
        roles: 'VOTER'
      },
      roleOptions: [
        { value: 'ADMIN', text: 'کاربر ادمین' },
        { value: 'SUPERVISOR', text: 'کاربر نظارت' },
        { value: 'EXECUTIVE', text: 'کاربر اجرایی' },
        { value: 'CANDIDATE', text: 'کاندید' },
        { value: 'VOTER', text: 'کاربر عادی' }
      ]
    }
  },
  computed: {
    ...mapGetters(["currentUser"]),
    currentUserRole() {
      return this.currentUser?.roles?.[0] || '';
    },
    currentUserRegionId() {
      return String(this.currentUser?.regionId || '');
    },
    isProvinceSupervisor() {
      return this.currentUserRole === 'SUPERVISOR' && this.currentUserRegionId.endsWith('00');
    },
    currentUserProvinceCode() {
      return this.isProvinceSupervisor ? this.currentUserRegionId.slice(0, -2) : '';
    },
    visibleProvinces() {
      if (!this.isProvinceSupervisor) return this.provinces;
      return this.provinces.filter(province => String(province.id) === this.currentUserProvinceCode);
    },
    selectedProvinceAreas() {
      return this.areasByProvince[this.selectedProvinceCode] || [];
    },
    filteredUsers() {
      const search = this.filters.search.toLowerCase();

      return this.users.filter(user => {
        const matchesRole = !this.filters.role || user.roles === this.filters.role;
        const matchesProvince = !this.filters.provinceCode || String(user.provinceCode) === this.filters.provinceCode;
        const searchableText = [
          user.national_id,
          user.first_name,
          user.last_name,
          user.regionName,
          user.personnel_code,
          user.regionName,
          user.provinceName,
          user.roles,
          this.getRoleName(user.roles)
        ].filter(Boolean).join(' ').toLowerCase();

        return matchesRole && matchesProvince && (!search || searchableText.includes(search));
      });
    }
  },
  mounted() {
    this.loadInitialData();
  },
  methods: {
    ...mapMutations(["setsidebarVisible"]),
    ...mapActions(["getUsers", "updateUser", "getRegions"]),
    async loadInitialData() {
      this.loading = true;
      try {
        const [users, regionsResponse] = await Promise.all([
          this.getUsers({ limit: 1000 }),
          this.getRegions()
        ]);

        this.users = Array.isArray(users) ? users : [];
        this.provinces = regionsResponse?.data || [];
        this.areasByProvince = regionsResponse?.areasByProvince || {};
        if (this.isProvinceSupervisor) {
          this.filters.provinceCode = this.currentUserProvinceCode;
        }
      } catch (error) {
        console.error("Error loading advertisements:", error);
        this.$bvToast.toast("خطا در بارگذاری کاربران", {
          title: "خطا",
          variant: "danger",
          solid: true
        });
      } finally {
        this.loading = false;
      }
    },
    editUser(user) {
      this.formData = {
        national_id: user.national_id,
        personnel_code: user.personnel_code,
        region_id: user.region_id ? String(user.region_id) : '',
        roles: user.roles || 'VOTER'
      };
      this.selectedProvinceCode = user.provinceCode ? String(user.provinceCode) : this.findProvinceCodeByRegion(user.region_id);
      this.editMode = true;
      this.$nextTick(() => window.scrollTo({ top: 0, behavior: 'smooth' }));
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
      if (!this.formData.national_id || !this.formData.region_id || !this.formData.roles) {
        this.showToast('لطفاً نقش و منطقه کاربر را کامل انتخاب کنید.', 'warning');
        return;
      }
      if (this.isProvinceSupervisor && String(this.selectedProvinceCode) !== this.currentUserProvinceCode) {
        this.showToast('اعضای هیأت نظارت استانی فقط مجاز به ویرایش کاربران استان خود هستند.', 'warning');
        return;
      }

      this.saving = true;
      try {
        const response = await this.updateUser({
          national_id: this.formData.national_id,
          region_id: this.formData.region_id,
          roles: this.formData.roles
        });

        if (response === false) {
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
      this.formData = {
        national_id: '',
        personnel_code: '',
        region_id: '',
        roles: 'VOTER'
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
      return roles[role] || role || '---';
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