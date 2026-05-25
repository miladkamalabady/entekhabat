<template>
  <div class="px-2 advertise-page-container">
    <b-container fluid class="advertisement-wrapper">
      
      <!-- هشدار ممنوعیت تبلیغات -->
      <div v-if="isAdsLocked" class="alert-locked mb-4">
        <div class="alert-locked-icon">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none">
            <path d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
            <path d="M18 6L6 18" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
        </div>
        <div class="alert-locked-content">
          <h6>محدودیت زمانی</h6>
          <p>24 ساعت مانده به شروع رای‌گیری، امکان ایجاد و ویرایش تبلیغات وجود ندارد.</p>
        </div>
      </div>

      <!-- Page Header with Gradient -->
      <div class="page-header-modern mb-4">
        <div class="page-header-content">
          <div>
            <h4 class="mb-1">📢 مدیریت تبلیغات</h4>
            <p class="mb-0 opacity-75">ایجاد و مدیریت تبلیغات انتخابات</p>
          </div>
          <button class="btn-primary-modern" :disabled="isAdsLocked" @click="showCreateModal = true">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <path d="M12 5V19M5 12H19" stroke-linecap="round"/>
            </svg>
            ایجاد تبلیغ جدید
          </button>
        </div>
      </div>

      <!-- Filters Card -->
      <div class="card-filter-modern mb-4" v-if="!isAdsLocked">
        <div class="filter-header">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path d="M3 6H21M6 12H18M10 18H14" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <span>فیلترها</span>
        </div>
        <div class="filter-body">
          <div class="filter-item">
            <label>وضعیت</label>
            <select v-model="filters.status" class="form-select-modern" @change="loadAdvertisements">
              <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">{{ opt.text }}</option>
            </select>
          </div>
        </div>
      </div>

      <!-- Table Card -->
      <div class="card-table-modern" v-if="!isAdsLocked">
        <div class="table-responsive-modern">
          <b-table :items="advertisements?.filter(x => x.deleter != 'CANDIDATE')" :fields="fields" :busy="loading"
            striped hover class="table-modern" responsive="md">
            
            <template #cell(image)="data">
              <div class="ad-image-cell" v-if="data.value">
                <img :src="`${apiUrlrtb}/${data.value}`" class="ad-thumbnail-modern" />
              </div>
              <span v-else class="no-image-badge">بدون تصویر</span>
            </template>

            <template #cell(type)="data">
              <span class="badge-type" :class="`type-${data.value}`">
                {{ getTypeText(data.value) }}
              </span>
            </template>

            <template #cell(status)="data">
              <span class="badge-status" :class="`status-${!data.item.deleter ? data.item.status : 'deleted'}`">
                {{ getStatusText(data.item) }}
              </span>
            </template>

            <template #cell(actions)="data">
              <div class="action-buttons">
                <button class="action-btn view" @click="viewAd(data.item)" title="مشاهده">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M1 12C1 12 5 4 12 4C19 4 23 12 23 12C23 12 19 20 12 20C5 20 1 12 1 12Z" stroke-width="1.5"/>
                    <circle cx="12" cy="12" r="3" stroke-width="1.5"/>
                  </svg>
                </button>
                <button v-if="!data.item.deleter" class="action-btn edit" @click="editAd(data.item)" title="ویرایش">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M17 3L21 7L7 21H3V17L17 3Z" stroke-width="1.5" stroke-linejoin="round"/>
                  </svg>
                </button>
                <button v-if="!data.item.deleter" class="action-btn delete" @click="deleteAd(data.item.id)" title="حذف">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M4 7H20M10 11V16M14 11V16M5 7L6 19C6 20.1 6.9 21 8 21H16C17.1 21 18 20.1 18 19L19 7M9 7V4C9 3.4 9.4 3 10 3H14C14.6 3 15 3.4 15 4V7" stroke-width="1.5" stroke-linecap="round"/>
                  </svg>
                </button>
              </div>
            </template>

            <template #table-busy>
              <div class="loading-overlay">
                <div class="spinner-modern"></div>
                <span>در حال بارگذاری...</span>
              </div>
            </template>
          </b-table>
        </div>

        <div class="pagination-modern" v-if="totalRows > perPage">
          <b-pagination v-model="currentPage" :total-rows="totalRows" :per-page="perPage" align="center"
            @input="loadAdvertisements" pills></b-pagination>
        </div>
      </div>
    </b-container>

    <!-- Create/Edit Modal - Improved -->
    <b-modal v-model="showCreateModal" :title="isEditing ? '✏️ ویرایش تبلیغ' : '➕ ایجاد تبلیغ جدید'" size="lg" hide-footer
      centered body-class="p-0" header-class="modal-header-modern" class="modal-glass">
      <div class="modal-body-modern">
        <b-form @submit.prevent="saveAd">
          <!-- فیلدها با استایل مدرن -->
          <div class="form-group-modern">
            <label>عنوان تبلیغ <span class="required">*</span></label>
            <input type="text" v-model="form.title" class="input-modern" placeholder="عنوان تبلیغ را وارد کنید" required>
          </div>

          <div class="form-group-modern">
            <label>زندگی‌نامه (بیوگرافی) مختصر <span class="required">*</span></label>
            <textarea v-model="form.description" rows="4" class="textarea-modern" placeholder="بیوگرافی خود را وارد کنید" required></textarea>
          </div>

          <div class="row-modern">
            <div class="col-modern">
              <div class="form-group-modern">
                <label>سوابق اجرایی مدیریتی</label>
                <textarea v-model="form.managerialRecords" rows="3" class="textarea-modern" placeholder="سوابق اجرایی مدیریتی را وارد کنید"></textarea>
              </div>
            </div>
            <div class="col-modern">
              <div class="form-group-modern">
                <label>سوابق علمی / پژوهشی</label>
                <textarea v-model="form.academicRecords" rows="3" class="textarea-modern" placeholder="سوابق علمی / پژوهشی را وارد کنید"></textarea>
              </div>
            </div>
          </div>

          <div class="row-modern">
            <div class="col-modern">
              <div class="form-group-modern">
                <label>مدارج و افتخارات</label>
                <textarea v-model="form.honors" rows="3" class="textarea-modern" placeholder="مدارج و افتخارات را وارد کنید"></textarea>
              </div>
            </div>
            <div class="col-modern">
              <div class="form-group-modern">
                <label>برنامه‌های نمایندگی</label>
                <textarea v-model="form.plans" rows="3" class="textarea-modern" placeholder="برنامه‌ها را وارد کنید"></textarea>
              </div>
            </div>
          </div>

          <div class="form-group-modern">
            <label>شعار تبلیغاتی</label>
            <input type="text" v-model="form.slogan" class="input-modern" placeholder="شعار تبلیغاتی را وارد کنید">
          </div>

          <div class="form-group-modern">
            <label>تصویر تبلیغ</label>
            <div class="file-upload-modern">
              <input type="file" id="ad-image" accept="image/*" @change="handleImageUpload" class="file-input-hidden">
              <label for="ad-image" class="file-upload-label">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d="M12 4V20M20 12H4" stroke-width="1.5" stroke-linecap="round"/>
                </svg>
                <span>انتخاب تصویر یا بکشید اینجا</span>
              </label>
              <small class="form-hint">حداکثر حجم: 2 مگابایت</small>
            </div>
            <div v-if="form.imagePreview" class="image-preview-modern">
              <img :src="form.imagePreview" alt="پیش‌نمایش">
              <button type="button" class="remove-preview" @click="form.imagePreview = null; form.imageFile = null">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d="M18 6L6 18M6 6L18 18" stroke-width="2" stroke-linecap="round"/>
                </svg>
              </button>
            </div>
          </div>

          <div class="form-group-modern">
            <label>لینک هدف</label>
            <input type="url" v-model="form.targetLink" class="input-modern" placeholder="https://example.com">
          </div>

          <div v-if="!isEditing && hasExistingAdvertisements" class="alert-payment-modern">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z" stroke-width="1.5"/>
            </svg>
            <span>ثبت تبلیغ دوم به بعد نیازمند پرداخت است.</span>
          </div>

          <div v-if="!isEditing && hasExistingAdvertisements" class="checkbox-modern">
            <input type="checkbox" v-model="form.isPaid" id="isPaid">
            <label for="isPaid">پرداخت تبلیغ دوم انجام شده است</label>
          </div>

          <div class="modal-actions">
            <button type="button" class="btn-outline-modern" @click="showCreateModal = false">انصراف</button>
            <button type="submit" class="btn-primary-modern" :disabled="saving">
              <span v-if="saving" class="spinner-small"></span>
              {{ isEditing ? 'ذخیره تغییرات' : 'ایجاد تبلیغ' }}
            </button>
          </div>
        </b-form>
      </div>
    </b-modal>

    <!-- View Modal - Improved -->
    <b-modal v-model="showViewModal" title="👁️ مشاهده تبلیغ" size="lg" hide-footer centered
      body-class="p-0" header-class="modal-header-modern" class="modal-glass">
      <div class="modal-body-modern view-modal-body" v-if="selectedAd">
        <div class="view-header">
          <h5>{{ selectedAd.title || 'بدون عنوان' }}</h5>
          <span class="badge-status" :class="`status-${!selectedAd.deleter ? selectedAd.status : 'deleted'}`">
            {{ getStatusText(selectedAd) }}
          </span>
        </div>

        <div class="view-image">
          <img v-if="selectedAd.image" :src="getImageUrl(selectedAd.image)" alt="تصویر تبلیغ">
          <div v-else class="no-image-placeholder">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <rect x="3" y="3" width="18" height="18" rx="2" stroke-width="1.5"/>
              <circle cx="8.5" cy="8.5" r="1.5" fill="currentColor"/>
              <path d="M21 15L16 10L5 21" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
            <span>تصویری ثبت نشده است</span>
          </div>
        </div>

        <div class="view-info">
          <div class="info-row">
            <strong>بیوگرافی:</strong>
            <p>{{ selectedAd.description || 'توضیحاتی ثبت نشده است.' }}</p>
          </div>
          <div class="info-row">
            <strong>سوابق اجرایی مدیریتی:</strong>
            <p>{{ selectedAd.managerialRecords || 'سوابقی یافت نشد.' }}</p>
          </div>
          <div class="info-row">
            <strong>سوابق علمی / پژوهشی:</strong>
            <p>{{ selectedAd.academicRecords || 'سوابقی یافت نشد.' }}</p>
          </div>
          <div class="info-row">
            <strong>مدارج و افتخارات:</strong>
            <p>{{ selectedAd.honors || 'مدارجی یافت نشد.' }}</p>
          </div>
          <div class="info-row">
            <strong>برنامه‌ها:</strong>
            <p>{{ selectedAd.plans || 'برنامه‌ای یافت نشد.' }}</p>
          </div>
          <div class="info-row">
            <strong>شعار تبلیغاتی:</strong>
            <p>{{ selectedAd.slogan || 'شعاری یافت نشد.' }}</p>
          </div>
        </div>

        <div class="view-footer">
          <div class="views-count">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M1 12C1 12 5 4 12 4C19 4 23 12 23 12C23 12 19 20 12 20C5 20 1 12 1 12Z" stroke-width="1.5"/>
              <circle cx="12" cy="12" r="3" stroke-width="1.5"/>
            </svg>
            {{ selectedAd.views || 0 }} بازدید
          </div>
          <a v-if="selectedAd.targetLink" :href="selectedAd.targetLink" target="_blank" class="link-modern">
            مشاهده لینک هدف
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M18 13V19C18 20.1 17.1 21 16 21H5C3.9 21 3 20.1 3 19V8C3 6.9 3.9 6 5 6H11M15 3H21M21 3V9M21 3L10 14" stroke-width="1.5" stroke-linecap="round"/>
            </svg>
          </a>
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script>
// اسکریپت شما بدون تغییر باقی می‌ماند
// فقط در data() مقدار filters.status را به '' تغییر ندهید
// همان کد قبلی شما اینجا قرار می‌گیرد
import { isMobile } from "../../utils";
import { mapGetters, mapMutations, mapActions } from "vuex";
import Sidebar from "../../navs/Sidebar.vue";
import { apiUrlrtb } from '../../constants/config'
export default {
  name: "AdvertisementManagement",
  components: { Sidebar },
  data() {
    return {
      apiUrlrtb,
      isMobile,
      filters: { type: "", status: "" },
      adTypes: [
        { value: "banner", text: "بنر" },
        { value: "video", text: "ویدیو" },
        { value: "text", text: "متنی" },
        { value: "popup", text: "پاپ‌آپ" }
      ],
      statusOptions: [
        { value: "", text: "همه" },
        { value: "active", text: "فعال" },
        { value: "inactive", text: "غیرفعال" },
      ],
      fields: [
        { key: "id", label: "شناسه", sortable: true },
        { key: "image", label: "تصویر" },
        { key: "title", label: "عنوان", sortable: true },
        { key: "status", label: "وضعیت", sortable: true },
        { key: "actions", label: "عملیات" }
      ],
      advertisements: [],
      loading: false,
      currentPage: 1,
      perPage: 10,
      totalRows: 0,
      showCreateModal: false,
      showViewModal: false,
      isEditing: false,
      saving: false,
      selectedAd: null,
      form: {
        id: null,
        title: "",
        description: "",
        type: "banner",
        imageFile: null,
        imagePreview: null,
        targetLink: "",
        status: "pending",
        managerialRecords: "",
        academicRecords: "",
        honors: "",
        plans: "",
        slogan: "",
        isPaid: false
      }
    };
  },
  computed: {
    ...mapGetters(["sidebarVisible", "ConfigInfo", "SystemScheduleInfo"]),
    votingStartDate() {
      const votingEvent = (this.SystemScheduleInfo || []).find(item => item?.event_key === "voting");
      if (votingEvent?.start_date) {
        return this.$moment(votingEvent.start_date, "jYYYY-jMM-jDD HH:mm:ss");
      }
      if (this.ConfigInfo?.startDate) {
        return this.$moment(this.ConfigInfo.startDate);
      }
      return null;
    },
    isAdsLocked() {
      if (!this.votingStartDate || !this.votingStartDate.isValid()) {
        return false;
      }
      const lockStart = this.votingStartDate.clone().subtract(24, "hours");
      return this.$moment().isSameOrAfter(lockStart);
    },
    hasExistingAdvertisements() {
      return this.advertisements.filter(ad => !ad.deleter).length >= 1;
    }
  },
  async created() {
    await this.getSystemSchedule();
    this.loadAdvertisements();
  },
  methods: {
    ...mapMutations(["setsidebarVisible"]),
    ...mapActions(["advertisementsSave", "getAdvertisements", "deleteAdv", "getSystemSchedule"]),
    async loadAdvertisements() {
      this.loading = true;
      try {
        this.advertisements = await this.getAdvertisements()
        this.totalRows = this.advertisements.length;
      } catch (error) {
        console.error("Error loading advertisements:", error);
        this.$bvToast.toast("خطا در بارگذاری تبلیغات", {
          title: "خطا",
          variant: "danger",
          solid: true
        });
      } finally {
        this.loading = false;
      }
    },
    handleImageUpload(event) {
      const file = event.target.files[0];
      if (file) {
        if (file.size > 2 * 1024 * 1024) {
          this.$bvToast.toast("حجم تصویر نباید بیشتر از 2 مگابایت باشد", {
            title: "خطا",
            variant: "danger",
            solid: true
          });
          this.form.imageFile = null;
          return;
        }
        const reader = new FileReader();
        reader.onload = e => {
          this.form.imagePreview = e.target.result;
        };
        reader.readAsDataURL(file);
      }
    },
    getTypeBadge(type) {
      const variants = { banner: "primary", video: "success", text: "info", popup: "warning" };
      return variants[type] || "secondary";
    },
    getImageUrl(imagePath) {
      if (!imagePath) return "";
      if (imagePath.startsWith("http://") || imagePath.startsWith("https://") || imagePath.startsWith("data:")) {
        return imagePath;
      }
      return `${apiUrlrtb}/${imagePath}`;
    },
    getStatusBadge(status) {
      const variants = { active: "success", inactive: "danger", pending: "warning" };
      return !status.deleter ? (variants[status.status] || variants['inactive']) : variants['inactive'];
    },
    getTypeText(type) {
      const typeMap = { banner: "بنر", video: "ویدیو", text: "متنی", popup: "پاپ‌آپ" };
      return typeMap[type] || type;
    },
    getStatusText(status) {
      const statusMap = { active: "فعال", delete: "حذف مدیر", inactive: "غیرفعال", pending: "در انتظار تایید" };
      return !status.deleter ? (statusMap[status.status] || statusMap['delete']) : statusMap['delete'];
    },
    viewAd(ad) {
      this.selectedAd = ad;
      this.showViewModal = true;
    },
    editAd(ad) {
      this.isEditing = true;
      this.selectedAd = ad;
      this.form = {
        id: ad.id,
        title: ad.title,
        description: ad.description,
        type: ad.type,
        imageFile: null,
        imagePreview: apiUrlrtb + '/' + ad.image,
        targetLink: ad.targetLink || "",
        status: ad.status,
        managerialRecords: ad.managerialRecords || "",
        academicRecords: ad.academicRecords || "",
        honors: ad.honors || "",
        plans: ad.plans || "",
        slogan: ad.slogan || "",
        isPaid: ad.isPaid || 0
      };
      this.showCreateModal = true;
    },
    async deleteAd(id) {
      if (this.isAdsLocked) return;
      if (!confirm("آیا از حذف این تبلیغ اطمینان دارید؟")) return;
      try {
        this.advertisements = this.advertisements.filter(ad => ad.id !== id);
        await this.deleteAdv({ code: id })
        this.$bvToast.toast("تبلیغ با موفقیت حذف شد", {
          title: "موفقیت",
          variant: "success",
          solid: true
        });
      } catch (error) {
        console.error("Error deleting advertisement:", error);
        this.$bvToast.toast("خطا در حذف تبلیغ", {
          title: "خطا",
          variant: "danger",
          solid: true
        });
      }
    },
    async saveAd() {
      if (this.isAdsLocked) {
        this.$bvToast.toast("24 ساعت مانده به شروع رای گیری : تبلیغات ممنوع", {
          title: "محدودیت زمانی",
          variant: "danger",
          solid: true
        });
        return;
      }
      this.saving = true;
      try {
        const formData = new FormData();
        formData.append("id", this.form.id || "");
        formData.append("title", this.form.title);
        formData.append("description", this.form.description);
        formData.append("type", this.form.type);
        formData.append("status", 'pending');
        formData.append("targetLink", this.form.targetLink || "");
        formData.append("managerialRecords", this.form.managerialRecords || "");
        formData.append("academicRecords", this.form.academicRecords || "");
        formData.append("honors", this.form.honors || "");
        formData.append("plans", this.form.plans || "");
        formData.append("slogan", this.form.slogan || "");
        formData.append("isPaid", this.form.isPaid ? "1" : "0");
        if (this.form.imageFile) {
          formData.append("image", this.form.imageFile);
        } else if (this.form.imagePreview && !this.form.imagePreview.startsWith("data:")) {
          formData.append("imagePath", this.form.imagePreview);
        }
        const response = await this.advertisementsSave(formData);
        if (!response?.status) {
          throw new Error(response?.message || "خطا در ذخیره تبلیغ");
        }
        const savedAd = {
          id: response.data?.id || this.form.id || this.advertisements.length + 1,
          title: this.form.title,
          description: this.form.description,
          type: this.form.type,
          image: response.data?.image || this.form.imagePreview,
          status: 'pending',
          targetLink: this.form.targetLink,
          managerialRecords: this.form.managerialRecords,
          academicRecords: this.form.academicRecords,
          honors: this.form.honors,
          plans: this.form.plans,
          slogan: this.form.slogan,
          views: this.form.views || 0
        };
        if (this.isEditing) {
          const index = this.advertisements.findIndex(ad => ad.id === this.form.id);
          if (index !== -1) {
            this.advertisements[index] = { ...this.advertisements[index], ...savedAd };
          }
          this.$bvToast.toast("تبلیغ با موفقیت ویرایش شد", { title: "موفقیت", variant: "success", solid: true });
        } else {
          this.advertisements.unshift(savedAd);
          this.$bvToast.toast("تبلیغ جدید با موفقیت ایجاد شد", { title: "موفقیت", variant: "success", solid: true });
        }
        this.resetForm();
        this.showCreateModal = false;
        this.isEditing = false;
      } catch (error) {
        console.error("Error saving advertisement:", error);
        this.$bvToast.toast(error?.message || "خطا در ذخیره تبلیغ", { title: "خطا", variant: "danger", solid: true });
      } finally {
        this.saving = false;
      }
    },
    resetForm() {
      this.form = {
        id: null,
        title: "",
        description: "",
        type: "banner",
        imageFile: null,
        imagePreview: null,
        targetLink: "",
        status: "pending",
        managerialRecords: "",
        academicRecords: "",
        honors: "",
        plans: "",
        slogan: "",
        isPaid: false
      };
    }
  }
};
</script>

<style scoped>
/* ========== استایل‌های مدرن برای صفحه تبلیغات ========== */

/* فاصله از topbar */
.advertise-page-container {
  margin-top: 70px;
}

.advertisement-wrapper {
  background: linear-gradient(135deg, #f5f7ff 0%, #eef2fa 100%);
  min-height: calc(100vh - 70px);
  padding: 24px;
  border-radius: 0;
}

/* ========== هشدار ممنوعیت ========== */
.alert-locked {
  background: linear-gradient(135deg, #fef3c7, #fde68a);
  border-radius: 20px;
  padding: 16px 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  color: #92400e;
  border-right: 4px solid #f59e0b;
}

.alert-locked-icon svg {
  width: 32px;
  height: 32px;
}

.alert-locked-content h6 {
  font-size: 0.9rem;
  font-weight: 700;
  margin-bottom: 4px;
}

.alert-locked-content p {
  font-size: 0.8rem;
  margin: 0;
  opacity: 0.8;
}

/* ========== هدر صفحه ========== */
.page-header-modern {
  background: linear-gradient(120deg, #1e2a6e, #2b3b8a);
  border-radius: 24px;
  padding: 20px 28px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
  color: white;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.1);
}

.page-header-content h4 {
  font-weight: 700;
}

/* ========== کارت فیلتر ========== */
.card-filter-modern {
  background: white;
  border-radius: 20px;
  padding: 16px 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.filter-header {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
  font-weight: 600;
  color: #1e293b;
}

.filter-body {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
}

.filter-item {
  min-width: 180px;
}

.filter-item label {
  display: block;
  font-size: 0.75rem;
  font-weight: 600;
  color: #64748b;
  margin-bottom: 6px;
}

.form-select-modern {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 0.85rem;
  background: white;
  transition: all 0.2s;
}

.form-select-modern:focus {
  outline: none;
  border-color: #3f51b5;
  box-shadow: 0 0 0 3px rgba(63, 81, 181, 0.1);
}

/* ========== کارت جدول ========== */
.card-table-modern {
  background: white;
  border-radius: 24px;
  padding: 0;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.06);
}

.table-responsive-modern {
  overflow-x: auto;
}

.table-modern {
  width: 100%;
  border-collapse: collapse;
}

.table-modern th {
  background: #f8fafc;
  padding: 14px 16px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #475569;
  border-bottom: 1px solid #e2e8f0;
}

.table-modern td {
  padding: 14px 16px;
  font-size: 0.85rem;
  vertical-align: middle;
  border-bottom: 1px solid #f1f5f9;
}

/* تصویر در جدول */
.ad-image-cell {
  width: 50px;
  height: 40px;
  overflow: hidden;
  border-radius: 10px;
}

.ad-thumbnail-modern {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.no-image-badge {
  font-size: 0.7rem;
  color: #94a3b8;
}

/* Badge نوع */
.badge-type {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 30px;
  font-size: 0.7rem;
  font-weight: 500;
}

.type-banner { background: #dbeafe; color: #1e40af; }
.type-video { background: #dcfce7; color: #166534; }
.type-text { background: #e0e7ff; color: #3730a3; }
.type-popup { background: #fed7aa; color: #9a3412; }

/* Badge وضعیت */
.badge-status {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 30px;
  font-size: 0.7rem;
  font-weight: 500;
}

.status-active { background: #dcfce7; color: #166534; }
.status-inactive { background: #fee2e2; color: #991b1b; }
.status-pending { background: #fef3c7; color: #92400e; }
.status-deleted { background: #f1f5f9; color: #475569; }

/* دکمه‌های عملیات */
.action-buttons {
  display: flex;
  gap: 8px;
}

.action-btn {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn.view { background: #e0e7ff; color: #3f51b5; }
.action-btn.edit { background: #fef3c7; color: #d97706; }
.action-btn.delete { background: #fee2e2; color: #ef4444; }

.action-btn:hover {
  transform: scale(1.05);
}

/* لودینگ */
.loading-overlay {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 48px;
  gap: 12px;
  color: #3f51b5;
}

.spinner-modern {
  width: 40px;
  height: 40px;
  border: 3px solid #e2e8f0;
  border-top-color: #3f51b5;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* پیجینیشن */
.pagination-modern {
  padding: 20px;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: center;
}

/* ========== مودال مدرن ========== */
.modal-glass ::v-deep .modal-content {
  background: white;
  border-radius: 32px;
  border: none;
  overflow: hidden;
  box-shadow: 0 24px 48px rgba(0, 0, 0, 0.2);
}

.modal-header-modern {
  background: linear-gradient(120deg, #1e2a6e, #2b3b8a);
  color: white;
  padding: 20px 24px;
  border: none;
}

.modal-header-modern .close {
  color: white;
  opacity: 0.8;
}

.modal-body-modern {
  padding: 28px;
}

/* فرم مدرن */
.form-group-modern {
  margin-bottom: 20px;
}

.form-group-modern label {
  display: block;
  font-size: 0.8rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
}

.required { color: #ef4444; margin-right: 4px; }

.input-modern,
.textarea-modern {
  width: 100%;
  padding: 12px 16px;
  border: 1.5px solid #e2e8f0;
  border-radius: 14px;
  font-size: 0.85rem;
  transition: all 0.2s;
  font-family: inherit;
}

.input-modern:focus,
.textarea-modern:focus {
  outline: none;
  border-color: #3f51b5;
  box-shadow: 0 0 0 3px rgba(63, 81, 181, 0.1);
}

.textarea-modern {
  resize: vertical;
}

.row-modern {
  display: flex;
  gap: 20px;
  margin-bottom: 0;
}

.col-modern {
  flex: 1;
}

/* آپلود فایل */
.file-upload-modern {
  position: relative;
}

.file-input-hidden {
  display: none;
}

.file-upload-label {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px;
  border: 2px dashed #cbd5e1;
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.2s;
  background: #f8fafc;
  color: #64748b;
}

.file-upload-label:hover {
  border-color: #3f51b5;
  background: #eef2ff;
}

.form-hint {
  display: block;
  font-size: 0.7rem;
  color: #94a3b8;
  margin-top: 8px;
}

.image-preview-modern {
  position: relative;
  margin-top: 16px;
  display: inline-block;
}

.image-preview-modern img {
  max-width: 150px;
  max-height: 120px;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
}

.remove-preview {
  position: absolute;
  top: -8px;
  right: -8px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #ef4444;
  color: white;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* هشدار پرداخت */
.alert-payment-modern {
  background: #fef3c7;
  border-radius: 12px;
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  font-size: 0.85rem;
  color: #92400e;
}

.checkbox-modern {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 24px;
}

.checkbox-modern input {
  width: 18px;
  height: 18px;
}

/* دکمه‌های مودال */
.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid #e2e8f0;
}

.btn-outline-modern {
  padding: 10px 24px;
  border: 1.5px solid #cbd5e1;
  background: transparent;
  border-radius: 40px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-outline-modern:hover {
  border-color: #3f51b5;
  color: #3f51b5;
}

.btn-primary-modern {
  padding: 10px 28px;
  background: linear-gradient(135deg, #3f51b5, #2c3e8f);
  border: none;
  border-radius: 40px;
  color: white;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.btn-primary-modern:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 14px rgba(63, 81, 181, 0.3);
}

.btn-primary-modern:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-small {
  width: 16px;
  height: 16px;
  border: 2px solid white;
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

/* ========== مشاهده تبلیغ ========== */
.view-modal-body {
  padding: 0;
}

.view-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

.view-header h5 {
  margin: 0;
  font-weight: 700;
}

.view-image {
  padding: 24px;
  background: #f8fafc;
  text-align: center;
}

.view-image img {
  max-height: 280px;
  max-width: 100%;
  border-radius: 16px;
}

.no-image-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 40px;
  color: #94a3b8;
}

.view-info {
  padding: 0 24px;
}

.info-row {
  margin-bottom: 20px;
}

.info-row strong {
  display: block;
  font-size: 0.75rem;
  color: #64748b;
  margin-bottom: 6px;
}

.info-row p {
  margin: 0;
  font-size: 0.85rem;
  color: #1e293b;
  line-height: 1.6;
}

.view-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  margin-top: 20px;
}

.views-count {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  color: #64748b;
}

.link-modern {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  color: #3f51b5;
  text-decoration: none;
  font-weight: 500;
}

.link-modern:hover {
  text-decoration: underline;
}

/* ========== موبایل ========== */
@media (max-width: 768px) {
  .advertise-page-container {
    margin-top: 60px;
  }
  
  .advertisement-wrapper {
    padding: 16px;
  }
  
  .page-header-modern {
    flex-direction: column;
    text-align: center;
    padding: 20px;
  }
  
  .row-modern {
    flex-direction: column;
    gap: 0;
  }
  
  .modal-body-modern {
    padding: 20px;
  }
  
  .view-header {
    flex-direction: column;
    gap: 12px;
    text-align: center;
  }
  
  .view-footer {
    flex-direction: column;
    gap: 12px;
  }
}
</style>