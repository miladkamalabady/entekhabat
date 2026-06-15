<template>
  <div class="announcements-page">

    <!-- هدر -->
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h5 class="mb-0 fw-bold">
        <b-icon icon="megaphone" class="ml-2" />
        مدیریت اطلاعیه‌ها
      </h5>
      <b-button variant="primary" size="sm" @click="openForm()">
        <b-icon icon="plus-circle" class="ml-1" /> اطلاعیه جدید
      </b-button>
    </div>

    <!-- لیست -->
    <div v-if="loadingList" class="text-center py-5">
      <b-spinner variant="primary" />
    </div>

    <div v-else-if="!items.length" class="text-center text-muted py-5">
      <b-icon icon="inbox" font-scale="3" class="mb-3 d-block mx-auto" />
      هیچ اطلاعیه‌ای ثبت نشده است.
    </div>

    <b-card v-for="item in items" :key="item.id" class="mb-2 ann-card">
      <div class="d-flex justify-content-between align-items-start">
        <div class="flex-grow-1">
          <div class="d-flex align-items-center gap-2 mb-1 flex-wrap">
            <span class="font-weight-bold">{{ item.title }}</span>
            <b-badge :variant="scopeVariant(item.target_scope)">{{ scopeLabel(item.target_scope) }}</b-badge>
            <b-badge v-if="item.target_scope !== 'country'" variant="light" class="text-muted small">
              {{ targetSummary(item) }}
            </b-badge>
            <b-badge :variant="item.is_active ? 'success' : 'secondary'">
              {{ item.is_active ? 'فعال' : 'حذف شده' }}
            </b-badge>
          </div>
          <small class="text-muted">{{ item.created_at_shamsi }}</small>
        </div>
        <div class="d-flex gap-1">
          <b-button size="sm" variant="outline-secondary" @click="openForm(item)">
            <b-icon icon="pencil" />
          </b-button>
          <b-button size="sm" variant="outline-danger" @click="confirmDelete(item)">
            <b-icon icon="trash" />
          </b-button>
        </div>
      </div>
    </b-card>

    <!-- مودال فرم -->
    <b-modal
      v-model="showForm"
      :title="form.id ? 'ویرایش اطلاعیه' : 'اطلاعیه جدید'"
      size="lg"
      hide-footer
      @hidden="resetForm"
    >
      <b-form @submit.prevent="submitForm">

        <b-form-group label="عنوان اطلاعیه *">
          <b-form-input v-model="form.title" placeholder="عنوان را وارد کنید" required />
        </b-form-group>

        <b-form-group label="متن اطلاعیه *">
          <b-form-textarea v-model="form.content" rows="5" placeholder="متن اطلاعیه را وارد کنید..." required />
        </b-form-group>

        <b-form-group label="محدوده نمایش *">
          <b-form-select v-model="form.target_scope" :options="scopeOptions" required @change="onScopeChange" />
        </b-form-group>

        <!-- انتخاب استان (چند استان) -->
        <b-form-group v-if="form.target_scope === 'province' && provinceOptions.length" label="استان‌ها *">
          <div v-if="provinceOptions.length === 1" class="text-muted small p-2 border rounded">
            {{ provinceOptions[0].text }} (تنها استان شما)
          </div>
          <div v-else class="checkbox-grid border rounded p-3">
            <b-form-checkbox
              v-for="p in provinceOptions"
              :key="p.value"
              v-model="form.target_ids"
              :value="p.value"
              class="mb-1"
            >{{ p.text }}</b-form-checkbox>
          </div>
          <small v-if="form.target_ids.length === 0 && form.target_scope === 'province'" class="text-danger">
            حداقل یک استان انتخاب کنید.
          </small>
        </b-form-group>

        <!-- انتخاب منطقه (چند منطقه) -->
        <b-form-group v-if="form.target_scope === 'region'" label="مناطق *">
          <div v-if="regionOptions.length === 1" class="text-muted small p-2 border rounded">
            {{ regionOptions[0].text }} (تنها منطقه شما)
          </div>
          <div v-else class="checkbox-grid border rounded p-3">
            <b-form-checkbox
              v-for="r in regionOptions"
              :key="r.value"
              v-model="form.target_ids"
              :value="r.value"
              class="mb-1"
            >{{ r.text }}</b-form-checkbox>
          </div>
          <div v-if="regionOptions.length > 1" class="mt-1">
            <b-button size="sm" variant="link" class="p-0 ml-2" @click="selectAll">انتخاب همه</b-button>
            <b-button size="sm" variant="link" class="p-0" @click="form.target_ids = []">حذف انتخاب</b-button>
          </div>
          <small v-if="form.target_ids.length === 0 && form.target_scope === 'region'" class="text-danger">
            حداقل یک منطقه انتخاب کنید.
          </small>
        </b-form-group>

        <div class="d-flex justify-content-end gap-2 mt-3">
          <b-button variant="outline-secondary" @click="showForm = false">انصراف</b-button>
          <b-button type="submit" variant="primary" :disabled="saving || !isFormValid">
            <b-spinner v-if="saving" small class="ml-1" />
            {{ form.id ? 'ذخیره تغییرات' : 'ثبت اطلاعیه' }}
          </b-button>
        </div>
      </b-form>
    </b-modal>

    <!-- تایید حذف -->
    <b-modal
      v-model="showDelete"
      title="حذف اطلاعیه"
      @ok="doDelete"
      ok-title="بله، حذف شود"
      ok-variant="danger"
      cancel-title="انصراف"
    >
      آیا از حذف اطلاعیه «{{ deleteTarget && deleteTarget.title }}» مطمئن هستید؟
    </b-modal>

  </div>
</template>

<script>
import { mapActions, mapGetters } from "vuex";

export default {
  name: "AnnouncementsManage",
  data() {
    return {
      items: [],
      allRegions: [],
      loadingList: false,
      saving: false,
      showForm: false,
      showDelete: false,
      deleteTarget: null,
      form: { id: null, title: "", content: "", target_scope: "region", target_ids: [] },
    };
  },
  computed: {
    ...mapGetters(["currentUser"]),
    userRole()           { return this.currentUser?.roles?.[0] || this.currentUser?.role || ""; },
    isAdmin()            { return this.userRole === "ADMIN"; },
    isProvinceSupervisor() {
      return this.userRole === "SUPERVISOR" && String(this.currentUser?.region_id || "").endsWith("00");
    },
    myProvinceCode()     { return this.currentUser?.ProvinceCode || null; },
    myRegionId()         { return this.currentUser?.region_id || null; },

    scopeOptions() {
      const opts = [{ value: "region", text: "منطقه‌ای" }];
      if (this.isAdmin || this.isProvinceSupervisor)
        opts.push({ value: "province", text: "استانی" });
      if (this.isAdmin)
        opts.push({ value: "country", text: "سراسری (همه)" });
      return opts;
    },

    // لیست استان‌ها برای چک‌باکس
    provinceOptions() {
      const provinces = this.allRegions
        .filter(r => String(r.id).endsWith("00"))
        .map(r => ({ value: r.id, text: r.name }));
      if (this.isAdmin) return provinces;
      // ناظر استانی فقط استان خودش
      return provinces.filter(r => r.value == this.myProvinceCode);
    },

    // لیست مناطق برای چک‌باکس
    regionOptions() {
      const regions = this.allRegions
        .filter(r => !String(r.id).endsWith("00"))
        .map(r => ({ value: r.id, text: r.name }));
      if (this.isAdmin) return regions;
      if (this.isProvinceSupervisor)
        return regions.filter(r => r.ProvinceCode == this.myProvinceCode);
      return regions.filter(r => r.value == this.myRegionId);
    },

    isFormValid() {
      if (this.form.target_scope === "country") return true;
      return this.form.target_ids.length > 0;
    },
  },
  methods: {
    ...mapActions(["getMyAnnouncements", "saveAnnouncement", "deleteAnnouncement", "getRegions"]),

    scopeLabel(s)  { return { country: "سراسری", province: "استانی", region: "منطقه‌ای" }[s] || s; },
    scopeVariant(s){ return { country: "danger",  province: "warning",  region: "info"      }[s] || "secondary"; },

    targetSummary(item) {
      const ids = item.target_ids;
      if (!ids || !ids.length) return "";
      if (item.target_scope === "province") {
        return ids.map(id => {
          const p = this.allRegions.find(r => r.id == id);
          return p ? p.name : id;
        }).join("، ");
      }
      if (item.target_scope === "region") {
        if (ids.length === 1) {
          const r = this.allRegions.find(r => r.id == ids[0]);
          return r ? r.name : ids[0];
        }
        return `${ids.length} منطقه`;
      }
      return "";
    },

    selectAll() {
      if (this.form.target_scope === "province")
        this.form.target_ids = this.provinceOptions.map(p => p.value);
      else
        this.form.target_ids = this.regionOptions.map(r => r.value);
    },

    async loadItems() {
      this.loadingList = true;
      this.items = await this.getMyAnnouncements() || [];
      this.loadingList = false;
    },

    async loadRegions() {
      const res = await this.getRegions();
      const provs  = res?.data || [];
      const areas  = res?.areasByProvince || {};
      const all = [];
      provs.forEach(p => {
        all.push({ id: p.id, name: p.name, ProvinceCode: p.id });
        (areas[p.id] || []).forEach(r => all.push({ ...r, ProvinceCode: p.id }));
      });
      this.allRegions = all;
    },

    openForm(item = null) {
      if (item) {
        this.form = {
          id: item.id,
          title: item.title,
          content: item.content,
          target_scope: item.target_scope,
          target_ids: Array.isArray(item.target_ids) ? [...item.target_ids] : [],
        };
      } else {
        this.resetForm();
      }
      this.showForm = true;
    },

    resetForm() {
      const defaultIds = this.myRegionId ? [this.myRegionId] : [];
      this.form = { id: null, title: "", content: "", target_scope: "region", target_ids: defaultIds };
    },

    onScopeChange() {
      // وقتی scope عوض می‌شود، target_ids را ریست کن و پیش‌فرض بگذار
      if (this.form.target_scope === "country") {
        this.form.target_ids = [];
      } else if (this.form.target_scope === "province") {
        this.form.target_ids = this.isAdmin ? [] : (this.myProvinceCode ? [this.myProvinceCode] : []);
      } else {
        this.form.target_ids = (!this.isAdmin && !this.isProvinceSupervisor && this.myRegionId)
          ? [this.myRegionId]
          : [];
      }
    },

    async submitForm() {
      this.saving = true;
      const res = await this.saveAnnouncement({ ...this.form });
      this.saving = false;
      if (res?.status) {
        this.$bvToast.toast("اطلاعیه با موفقیت ذخیره شد.", { title: "موفق", variant: "success", solid: true });
        this.showForm = false;
        await this.loadItems();
      } else {
        this.$bvToast.toast(res?.message || "خطا در ذخیره اطلاعیه", { title: "خطا", variant: "danger", solid: true });
      }
    },

    confirmDelete(item) {
      this.deleteTarget = item;
      this.showDelete   = true;
    },

    async doDelete() {
      if (!this.deleteTarget) return;
      const res = await this.deleteAnnouncement({ id: this.deleteTarget.id });
      if (res?.status) {
        this.$bvToast.toast("اطلاعیه حذف شد.", { title: "موفق", variant: "success", solid: true });
        await this.loadItems();
      }
      this.deleteTarget = null;
    },
  },
  async mounted() {
    await Promise.all([this.loadItems(), this.loadRegions()]);
  },
};
</script>

<style scoped>
.ann-card { transition: 0.2s; }
.ann-card:hover { background: #f9fbff; }
.gap-1 { gap: 0.25rem; }
.gap-2 { gap: 0.5rem; }
.checkbox-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 4px;
  max-height: 260px;
  overflow-y: auto;
}
</style>
