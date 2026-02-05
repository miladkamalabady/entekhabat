<template>
  <div class="px-2">
    <b-container fluid class="advertisement-wrapper">
      <!-- Page Header -->
      <b-card class="mb-4">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <h4 class="mb-0">مدیریت تبلیغات</h4>
            <p class="text-muted mb-0">ایجاد و مدیریت تبلیغات انتخابات</p>
          </div>
          <b-button variant="primary" @click="showCreateModal = true">
            <b-icon icon="plus-circle" class="ml-1"></b-icon>
            ایجاد تبلیغ جدید
          </b-button>
        </div>
      </b-card>

      <!-- Filters -->
      <b-card class="mb-4">
        <h6 class="mb-3">فیلترها</h6>
        <b-row>
          <b-col md="3">
            <b-form-group label="نوع تبلیغ">
              <b-form-select v-model="filters.type" :options="adTypes" @change="loadAdvertisements"></b-form-select>
            </b-form-group>
          </b-col>
          <b-col md="3">
            <b-form-group label="وضعیت">
              <b-form-select v-model="filters.status" :options="statusOptions"
                @change="loadAdvertisements"></b-form-select>
            </b-form-group>
          </b-col>
          <b-col md="3">
            <b-form-group label="تاریخ شروع">
              <b-form-datepicker v-model="filters.startDate" :locale="'fa'"
                @input="loadAdvertisements"></b-form-datepicker>
            </b-form-group>
          </b-col>
          <b-col md="3">
            <b-form-group label="تاریخ پایان">
              <b-form-datepicker v-model="filters.endDate" :locale="'fa'"
                @input="loadAdvertisements"></b-form-datepicker>
            </b-form-group>
          </b-col>
        </b-row>
      </b-card>

      <!-- Advertisements List -->
      <b-card>
        <div class="table-responsive">
          <b-table :items="advertisements" :fields="fields" :busy="loading" striped hover class="text-right">
            <!-- Image Column -->
            <template #cell(image)="data">
              <img v-if="data.value" :src="data.value" class="ad-thumbnail" alt="تصویر تبلیغ" />
              <span v-else class="text-muted">بدون تصویر</span>
            </template>

            <!-- Type Column -->
            <template #cell(type)="data">
              <b-badge :variant="getTypeBadge(data.value)">
                {{ getTypeText(data.value) }}
              </b-badge>
            </template>

            <!-- Status Column -->
            <template #cell(status)="data">
              <b-badge :variant="getStatusBadge(data.value)">
                {{ getStatusText(data.value) }}
              </b-badge>
            </template>

            <!-- Actions Column -->
            <template #cell(actions)="data">
              <b-button-group size="sm">
                <b-button variant="outline-info" @click="viewAd(data.item)" title="مشاهده">
                  <b-icon icon="eye"></b-icon>
                </b-button>
                <b-button variant="outline-warning" @click="editAd(data.item)" title="ویرایش">
                  <b-icon icon="pencil"></b-icon>
                </b-button>
                <b-button variant="outline-danger" @click="deleteAd(data.item.id)" title="حذف">
                  <b-icon icon="trash"></b-icon>
                </b-button>
              </b-button-group>
            </template>

            <!-- Loading State -->
            <template #table-busy>
              <div class="text-center text-primary my-2">
                <b-spinner class="align-middle"></b-spinner>
                <strong>در حال بارگذاری...</strong>
              </div>
            </template>
          </b-table>
        </div>

        <!-- Pagination -->
        <b-pagination v-model="currentPage" :total-rows="totalRows" :per-page="perPage" align="center" class="mt-3"
          @input="loadAdvertisements"></b-pagination>
      </b-card>
    </b-container>

    <!-- Create/Edit Modal -->
    <b-modal v-model="showCreateModal" :title="isEditing ? 'ویرایش تبلیغ' : 'ایجاد تبلیغ جدید'" size="lg" hide-footer
      centered scrollable>
      <b-form @submit.prevent="saveAd">
        <b-row>
          <b-col md="6">
            <b-form-group label="عنوان تبلیغ" label-for="ad-title">
              <b-form-input id="ad-title" v-model="form.title" required
                placeholder="عنوان تبلیغ را وارد کنید"></b-form-input>
            </b-form-group>
          </b-col>
          <b-col md="6">
            <b-form-group label="نوع تبلیغ" label-for="ad-type">
              <b-form-select id="ad-type" v-model="form.type" :options="adTypes" required></b-form-select>
            </b-form-group>
          </b-col>
        </b-row>

        <b-form-group label="متن تبلیغ" label-for="ad-description">
          <b-form-textarea id="ad-description" v-model="form.description" rows="4" placeholder="متن تبلیغ را وارد کنید"
            required></b-form-textarea>
        </b-form-group>

        <b-row>
          <b-col md="6">
            <b-form-group label="تاریخ شروع" label-for="ad-start">
              <b-form-datepicker id="ad-start" v-model="form.startDate" :locale="'fa'" required></b-form-datepicker>
            </b-form-group>
          </b-col>
          <b-col md="6">
            <b-form-group label="تاریخ پایان" label-for="ad-end">
              <b-form-datepicker id="ad-end" v-model="form.endDate" :locale="'fa'" required></b-form-datepicker>
            </b-form-group>
          </b-col>
        </b-row>

        <b-form-group label="تصویر تبلیغ" label-for="ad-image">
          <b-form-file id="ad-image" v-model="form.imageFile" accept="image/*" @change="handleImageUpload"
            placeholder="تصویر را انتخاب کنید یا اینجا بکشید" drop-placeholder="تصویر را اینجا رها کنید"></b-form-file>
          <small class="form-text text-muted">حداکثر حجم: 2 مگابایت</small>

          <!-- Image Preview -->
          <div v-if="form.imagePreview" class="mt-3">
            <img :src="form.imagePreview" class="img-preview" alt="پیش‌نمایش تصویر" />
          </div>
        </b-form-group>

        <b-form-group label="لینک هدف" label-for="ad-link">
          <b-form-input id="ad-link" v-model="form.targetLink" type="url"
            placeholder="https://example.com"></b-form-input>
        </b-form-group>

        <b-form-group label="وضعیت">
          <b-form-radio-group v-model="form.status" :options="[
            { text: 'فعال', value: 'active' },
            { text: 'غیرفعال', value: 'inactive' },
            { text: 'در انتظار تایید', value: 'pending' }
          ]"></b-form-radio-group>
        </b-form-group>

        <div class="d-flex justify-content-end mt-4">
          <b-button variant="outline-secondary" class="ml-2" @click="showCreateModal = false">
            انصراف
          </b-button>
          <b-button type="submit" variant="primary" :disabled="saving">
            <b-spinner small v-if="saving" class="ml-1"></b-spinner>
            {{ isEditing ? 'ذخیره تغییرات' : 'ایجاد تبلیغ' }}
          </b-button>
        </div>
      </b-form>
    </b-modal>

    <!-- View Modal -->
    <b-modal v-model="showViewModal" title="مشاهده تبلیغ" size="lg" hide-footer centered>
      <div v-if="selectedAd">
        <b-row class="mb-3">
          <b-col md="8">
            <h5>{{ selectedAd.title }}</h5>
            <p class="text-muted">{{ selectedAd.description }}</p>
          </b-col>
          <b-col md="4">
            <img v-if="selectedAd.image" :src="selectedAd.image" class="img-fluid rounded" alt="تصویر تبلیغ" />
          </b-col>
        </b-row>

        <b-row class="mb-2">
          <b-col><strong>نوع:</strong> {{ getTypeText(selectedAd.type) }}</b-col>
          <b-col><strong>وضعیت:</strong> {{ getStatusText(selectedAd.status) }}</b-col>
        </b-row>

        <b-row class="mb-2">
          <b-col><strong>تاریخ شروع:</strong> {{ selectedAd.startDate }}</b-col>
          <b-col><strong>تاریخ پایان:</strong> {{ selectedAd.endDate }}</b-col>
        </b-row>

        <div v-if="selectedAd.targetLink" class="mb-2">
          <strong>لینک هدف:</strong>
          <a :href="selectedAd.targetLink" target="_blank">{{ selectedAd.targetLink }}</a>
        </div>

        <div class="mt-3">
          <strong>آمار بازدید:</strong> {{ selectedAd.views || 0 }} بازدید
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script>
import { isMobile } from "../../utils";
import { mapGetters, mapMutations } from "vuex";
import Sidebar from "../../navs/Sidebar.vue";

export default {
  name: "AdvertisementManagement",
  components: { Sidebar },
  data() {
    return {
      isMobile,
      // Filters
      filters: {
        type: "",
        status: "",
        startDate: null,
        endDate: null
      },
      // Ad Types
      adTypes: [
        { value: "banner", text: "بنر" },
        { value: "video", text: "ویدیو" },
        { value: "text", text: "متنی" },
        { value: "popup", text: "پاپ‌آپ" }
      ],
      // Status Options
      statusOptions: [
        { value: "", text: "همه" },
        { value: "active", text: "فعال" },
        { value: "inactive", text: "غیرفعال" },
        { value: "pending", text: "در انتظار تایید" }
      ],
      // Table Fields
      fields: [
        { key: "id", label: "شناسه", sortable: true },
        { key: "image", label: "تصویر" },
        { key: "title", label: "عنوان", sortable: true },
        { key: "type", label: "نوع", sortable: true },
        { key: "startDate", label: "تاریخ شروع", sortable: true },
        { key: "endDate", label: "تاریخ پایان", sortable: true },
        { key: "status", label: "وضعیت", sortable: true },
        { key: "actions", label: "عملیات" }
      ],
      // Advertisements Data
      advertisements: [],
      loading: false,
      currentPage: 1,
      perPage: 10,
      totalRows: 0,
      // Modal States
      showCreateModal: false,
      showViewModal: false,
      isEditing: false,
      saving: false,
      // Selected Ad for View/Edit
      selectedAd: null,
      // Form Data
      form: {
        id: null,
        title: "",
        description: "",
        type: "banner",
        startDate: null,
        endDate: null,
        imageFile: null,
        imagePreview: null,
        targetLink: "",
        status: "active"
      }
    };
  },
  computed: {
    ...mapGetters(["sidebarVisible"])
  },
  created() {
    this.loadAdvertisements();
    // Set default dates
    const today = new Date().toISOString().split("T")[0];
    const nextMonth = new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0];
    this.form.startDate = today;
    this.form.endDate = nextMonth;
  },
  methods: {
    ...mapMutations(["setsidebarVisible"]),

    // Load Advertisements
    async loadAdvertisements() {
      this.loading = true;
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 500));

        // Mock data
        this.advertisements = [
          {
            id: 1,
            title: "تبلیغ انتخابات فرهنگیان",
            description: "شرکت در انتخابات صندوق ذخیره فرهنگیان",
            type: "banner",
            image: "assets/img/adver/ent1.jpg",
            startDate: "1402/10/15",
            endDate: "1402/11/15",
            status: "active",
            targetLink: "https://example.com",
            views: 1245
          },
          {
            id: 2,
            title: "اعلام کاندیداتوری",
            description: "فراخوان کاندیداتوری برای انتخابات",
            type: "text",
            image: null,
            startDate: "1402/10/10",
            endDate: "1402/10/20",
            status: "pending",
            targetLink: "",
            views: 876
          }
        ];

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

    // Handle Image Upload
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

    // Get Badge Variants
    getTypeBadge(type) {
      const variants = {
        banner: "primary",
        video: "success",
        text: "info",
        popup: "warning"
      };
      return variants[type] || "secondary";
    },

    getStatusBadge(status) {
      const variants = {
        active: "success",
        inactive: "danger",
        pending: "warning"
      };
      return variants[status] || "secondary";
    },

    getTypeText(type) {
      const typeMap = {
        banner: "بنر",
        video: "ویدیو",
        text: "متنی",
        popup: "پاپ‌آپ"
      };
      return typeMap[type] || type;
    },

    getStatusText(status) {
      const statusMap = {
        active: "فعال",
        inactive: "غیرفعال",
        pending: "در انتظار تایید"
      };
      return statusMap[status] || status;
    },

    // CRUD Operations
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
        startDate: ad.startDate,
        endDate: ad.endDate,
        imageFile: null,
        imagePreview: ad.image,
        targetLink: ad.targetLink || "",
        status: ad.status
      };
      this.showCreateModal = true;
    },

    async deleteAd(id) {
      if (!confirm("آیا از حذف این تبلیغ اطمینان دارید؟")) return;

      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 300));

        this.advertisements = this.advertisements.filter(ad => ad.id !== id);
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
      this.saving = true;
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1000));

        if (this.isEditing) {
          // Update existing ad
          const index = this.advertisements.findIndex(ad => ad.id === this.form.id);
          if (index !== -1) {
            this.advertisements[index] = {
              ...this.advertisements[index],
              ...this.form,
              image: this.form.imagePreview
            };
          }

          this.$bvToast.toast("تبلیغ با موفقیت ویرایش شد", {
            title: "موفقیت",
            variant: "success",
            solid: true
          });
        } else {
          // Create new ad
          const newAd = {
            id: this.advertisements.length + 1,
            title: this.form.title,
            description: this.form.description,
            type: this.form.type,
            image: this.form.imagePreview,
            startDate: this.form.startDate,
            endDate: this.form.endDate,
            status: this.form.status,
            targetLink: this.form.targetLink,
            views: 0
          };

          this.advertisements.unshift(newAd);

          this.$bvToast.toast("تبلیغ جدید با موفقیت ایجاد شد", {
            title: "موفقیت",
            variant: "success",
            solid: true
          });
        }

        // Reset form and close modal
        this.resetForm();
        this.showCreateModal = false;
        this.isEditing = false;
      } catch (error) {
        console.error("Error saving advertisement:", error);
        this.$bvToast.toast("خطا در ذخیره تبلیغ", {
          title: "خطا",
          variant: "danger",
          solid: true
        });
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
        startDate: new Date().toISOString().split("T")[0],
        endDate: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0],
        imageFile: null,
        imagePreview: null,
        targetLink: "",
        status: "active"
      };
    }
  }
};
</script>

<style scoped>
.advertisement-wrapper {
  background: #f8f9fa;
  min-height: calc(100vh - 100px);
  border-radius: 8px;
}

.ad-thumbnail {
  width: 60px;
  height: 40px;
  object-fit: cover;
  border-radius: 4px;
  border: 1px solid #dee2e6;
}

.img-preview {
  max-width: 200px;
  max-height: 150px;
  object-fit: contain;
  border-radius: 6px;
  border: 1px solid #dee2e6;
  padding: 5px;
  background: #fff;
}

.table-responsive {
  border-radius: 6px;
  overflow: hidden;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .ad-thumbnail {
    width: 40px;
    height: 30px;
  }

  .fields th:nth-child(2),
  .fields td:nth-child(2) {
    display: none;
  }
}
</style>