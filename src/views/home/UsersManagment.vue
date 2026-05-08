<template>
  <div class="user-management">
    <h2>مدیریت کاربران</h2>
    
    <!-- فرم اضافه/ویرایش کاربر -->
    <div class="user-form" v-if="editMode">
      <h3>{{ 'ویرایش کاربر'}}</h3>
      <form @submit.prevent="saveUser">
        <div class="form-group">
          <label>کد منطقه:</label>
          <input type="text" v-model="formData.region_id" >
        </div>
        <div class="form-group">
          <label>نقش:</label>
          <select v-model="formData.roles">
            <option value="EXECUTIVE">کاربر اجرایی منطقه</option>
            <option value="SUPERVISOR">کاربر نظارت منطقه</option>
          </select>
        </div>
        <div class="form-buttons">
          <button type="submit">ذخیره</button>
          <button type="button" @click="cancelEdit">انصراف</button>
        </div>
      </form>
    </div>

    <!-- لیست کاربران -->
    <div class="user-list">
      <h3>لیست کاربران</h3>
      <table>
        <thead>
          <tr>
            <th>کد ملی</th>
            <th>کد پرسنلی</th>
            <th> استان</th>
            <th> منطقه</th>
            <th> تحصیلات</th>
            <th> سنوات</th>
            <th>نقش</th>
            <th>عملیات</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.id">
            <td>{{ user.national_id }}</td>
            <td>{{ user.personnel_code }}</td>
            <td>{{ user.provinceName }}</td>
            <td>{{ user.regionName }}</td>
            <td>{{ user.education }}</td>
            <td>{{ user.yearsOfService }}</td>
            
            <td>{{ getRoleName(user.roles) }}</td>
            <td>
              <button @click="editUser(user)">ویرایش</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script>
import { mapGetters, mapMutations, mapActions } from "vuex";
export default {
  name: 'UserManagement',
  data() {
    return {
      users: [],
      formData: {
        id: null,
        national_id: '',
        personnel_code: '',
        provinceCode: '',
        region_id: '',
        roles: 'ADMIN'
      },
      editMode: false,
      userRole: 'ADMIN' 
    }
  },
  computed: {
    filteredUsers() {
      if (this.userRole === 'ADMIN') {
        // کاربر ستاد: فقط کاربران استان را می‌بیند
        return this.users
      } else if (this.userRole === 'user_province') {
        // کاربر استان: فقط کاربران اجرایی و نظارت منطقه خودش را می‌بیند
        const currentProvinceCode = this.getCurrentUserProvinceCode();
        return this.users.filter(user => 
          (user.roles === 'EXECUTIVE' || user.roles === 'SUPERVISOR') && 
          user.provinceCode === currentProvinceCode
        );
      }
      return this.users;
    }
  },
  mounted() {
    this.loadUsers();
  },
  methods: {
    ...mapMutations(["setsidebarVisible"]),
    ...mapActions(["getUsers","updateUser"]),
    async loadUsers() {
    this.loading = true;
      try {
        this.users = await this.getUsers()
        this.totalRows = this.users.length;
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
    getCurrentUserProvinceCode() {
      // دریافت کد استان کاربر جاری
      // در حالت واقعی باید از لاگین کاربر دریافت شود
      return '01';
    },
   async saveUser() {
      if (this.editMode.national_id && this.editMode.personnel_code) {
        
        if (this.userRole === 'user_province') {
          this.formData.provinceCode = this.getCurrentUserProvinceCode();
        }
        
       await this.updateUser(this.formData);
        this.loadUsers();
        this.cancelEdit();
      } else {
        alert('لطفا کد ملی را وارد کنید');
      }
    },
    editUser(user) {
      this.formData = { ...user };
      this.editMode = user;
    },
    cancelEdit() {
      this.formData = {
        id: null,
        national_id: '',
        personnel_code: '',
        provinceCode: '',
        region_id: '',
        role: ''
      };
      this.editMode = false;
    },
    getRoleName(role) {
      const roles = {
        'EXECUTIVE': 'کاربر اجرایی منطقه',
        'SUPERVISOR': 'کاربر نظارت منطقه',
        'VOTER': 'کاربر',
        'CANDIDATE': 'کاندید',
        'ADMIN': 'کاربر ادمین'
      };
      return roles[role] || roles;
    }
  }
}
</script>

<style scoped>
.user-management {
  direction: rtl;
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.user-form {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.form-group {
  margin-bottom: 15px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.form-group label {
  width: 120px;
  font-weight: bold;
}

.form-group input, 
.form-group select {
  flex: 1;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  max-width: 300px;
}

.form-group input:disabled,
.form-group select:disabled {
  background-color: #e9ecef;
  cursor: not-allowed;
}

.form-buttons {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

button {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background-color 0.3s;
}

button[type="submit"] {
  background-color: #28a745;
  color: white;
}

button[type="submit"]:hover {
  background-color: #218838;
}

button[type="button"] {
  background-color: #6c757d;
  color: white;
}

button[type="button"]:hover {
  background-color: #5a6268;
}

.user-list button {
  background-color: #007bff;
  color: white;
  margin: 0 5px;
}

.user-list button:hover {
  background-color: #0056b3;
}

.user-list button:last-child {
  background-color: #dc3545;
}

.user-list button:last-child:hover {
  background-color: #c82333;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

th, td {
  padding: 12px;
  text-align: right;
  border-bottom: 1px solid #ddd;
}

th {
  background-color: #f8f9fa;
  font-weight: bold;
}

tr:hover {
  background-color: #f5f5f5;
}
</style>