<template>
  <div class="user-advertisements-page">
    <!-- Header Section -->
    <b-container fluid class="ads-header py-4 mb-4">
      <b-row class="align-items-center">
        <b-col cols="12" md="8">
          <h2 class="mb-2">تبلیغات انتخابات</h2>
          <p class="text-light mb-0">
            آخرین تبلیغات و اطلاعیه‌های مربوط به انتخابات صندوق ذخیره فرهنگیان
          </p>
        </b-col>
        <b-col cols="12" md="4" class="text-left text-md-right">
          <b-badge variant="info" class="p-2">
            <b-icon icon="megaphone" class="ml-1"></b-icon>
            {{ activeAdsCount }} تبلیغ فعال
          </b-badge>
        </b-col>
      </b-row>
    </b-container>

    <!-- Main Content -->
    <b-container class="ads-container">
      <!-- Search and Filter -->
      <b-card class="mb-4 filter-card">
        <b-row>
          <b-col md="6" class="mb-3 mb-md-0">
            <b-input-group>
              <template #prepend>
                <b-input-group-text>
                  <b-icon icon="search"></b-icon>
                </b-input-group-text>
              </template>
              <b-form-input
                v-model="searchQuery"
                placeholder="جستجوی در تبلیغات..."
                @input="filterAds"
              ></b-form-input>
              <template #append>
                <b-button variant="outline-secondary" @click="clearSearch">
                  پاک کردن
                </b-button>
              </template>
            </b-input-group>
          </b-col>
          <b-col md="6">
            <b-row>
              <b-col cols="6">
                <b-form-select
                  v-model="selectedType"
                  :options="filterTypes"
                  @change="filterAds"
                >
                  <template #first>
                    <option value="">همه انواع</option>
                  </template>
                </b-form-select>
              </b-col>
              <b-col cols="6">
                <b-form-select
                  v-model="sortBy"
                  :options="sortOptions"
                  @change="sortAds"
                ></b-form-select>
              </b-col>
            </b-row>
          </b-col>
        </b-row>
      </b-card>

      <!-- Active Ads Section -->
      <div class="mb-5">
        <h4 class="section-title mb-3">
          <b-icon icon="star-fill" variant="warning" class="ml-2"></b-icon>
          تبلیغات فعال
        </h4>
        
        <!-- Banner Ads Carousel -->
        <div v-if="bannerAds.length > 0" class="mb-4">
          <b-carousel
            id="ads-carousel"
            v-model="carouselSlide"
            :interval="5000"
            controls
            indicators
            background="#f8f9fa"
            img-width="1024"
            img-height="320"
            style="text-shadow: 1px 1px 2px #333;"
            @sliding-start="onSlideStart"
            @sliding-end="onSlideEnd"
          >
            <b-carousel-slide
              v-for="(ad, index) in bannerAds"
              :key="`banner-${ad.id}`"
              :img-src="ad.image || '/placeholder-banner.jpg'"
              :caption="ad.title"
              :text="ad.description"
              @click.native="viewAdDetails(ad)"
            >
              <template #img>
                <div class="carousel-image-wrapper" @click="viewAdDetails(ad)">
                  <img
                    class="d-block img-fluid w-100 carousel-image"
                    :src="ad.image || '/placeholder-banner.jpg'"
                    :alt="ad.title"
                  />
                  <div class="carousel-caption-overlay">
                    <h5>{{ ad.title }}</h5>
                    <p>{{ ad.description }}</p>
                    <small>تا تاریخ: {{ ad.endDate }}</small>
                  </div>
                </div>
              </template>
            </b-carousel-slide>
          </b-carousel>
        </div>

        <!-- Other Active Ads -->
        <b-row v-if="otherActiveAds.length > 0">
          <b-col
            v-for="ad in otherActiveAds"
            :key="`active-${ad.id}`"
            cols="12"
            md="6"
            lg="4"
            class="mb-4"
          >
            <b-card
              class="ad-card h-100"
              :class="{ 'featured-ad': ad.isFeatured }"
              @click="viewAdDetails(ad)"
            >
              <!-- Ad Badges -->
              <div class="ad-badges">
                <b-badge v-if="ad.isFeatured" variant="warning" class="ml-1">
                  ویژه
                </b-badge>
                <b-badge :variant="getTypeBadge(ad.type)" class="ml-1">
                  {{ getTypeText(ad.type) }}
                </b-badge>
              </div>

              <!-- Ad Image -->
              <div class="ad-image-container mb-3">
                <img
                  v-if="ad.image"
                  :src="ad.image"
                  class="ad-image"
                  :alt="ad.title"
                />
                <div v-else class="ad-image-placeholder">
                  <b-icon icon="image" font-scale="3"></b-icon>
                </div>
              </div>

              <!-- Ad Content -->
              <h5 class="ad-title">{{ ad.title }}</h5>
              <p class="ad-description text-muted">
                {{ truncateText(ad.description, 100) }}
              </p>

              <!-- Ad Footer -->
              <div class="ad-footer">
                <small class="text-muted">
                  <b-icon icon="calendar" class="ml-1"></b-icon>
                  تا {{ ad.endDate }}
                </small>
                <b-button
                  size="sm"
                  variant="outline-primary"
                  @click.stop="viewAdDetails(ad)"
                >
                  مشاهده جزئیات
                </b-button>
              </div>
            </b-card>
          </b-col>
        </b-row>

        <!-- No Active Ads -->
        <div v-if="filteredAds.length === 0" class="text-center py-5">
          <b-icon icon="info-circle" font-scale="4" variant="secondary"></b-icon>
          <h5 class="mt-3">هیچ تبلیغ فعالی یافت نشد</h5>
          <p class="text-muted">در حال حاضر تبلیغ فعالی برای نمایش وجود ندارد.</p>
        </div>
      </div>

      <!-- Upcoming Ads Section -->
      <div v-if="upcomingAds.length > 0" class="mb-5">
        <h4 class="section-title mb-3">
          <b-icon icon="clock-fill" variant="info" class="ml-2"></b-icon>
          تبلیغات آتی
        </h4>
        <b-row>
          <b-col
            v-for="ad in upcomingAds"
            :key="`upcoming-${ad.id}`"
            cols="12"
            md="6"
            class="mb-3"
          >
            <b-card class="upcoming-ad-card">
              <b-row no-gutters class="align-items-center">
                <b-col md="4" class="text-center">
                  <div class="upcoming-date">
                    <div class="date-day">{{ getDay(ad.startDate) }}</div>
                    <div class="date-month">{{ getMonth(ad.startDate) }}</div>
                  </div>
                </b-col>
                <b-col md="8">
                  <h6>{{ ad.title }}</h6>
                  <p class="text-muted small mb-2">
                    {{ truncateText(ad.description, 80) }}
                  </p>
                  <small class="text-info">
                    <b-icon icon="clock" class="ml-1"></b-icon>
                    شروع از {{ ad.startDate }}
                  </small>
                </b-col>
              </b-row>
            </b-card>
          </b-col>
        </b-row>
      </div>

      <!-- Statistics -->
      <b-card class="statistics-card mt-4">
        <h5 class="mb-3">آمار تبلیغات</h5>
        <b-row>
          <b-col cols="6" md="3" class="text-center mb-3">
            <div class="stat-item">
              <div class="stat-number text-primary">{{ totalAds }}</div>
              <div class="stat-label">کل تبلیغات</div>
            </div>
          </b-col>
          <b-col cols="6" md="3" class="text-center mb-3">
            <div class="stat-item">
              <div class="stat-number text-success">{{ activeAdsCount }}</div>
              <div class="stat-label">تبلیغات فعال</div>
            </div>
          </b-col>
          <b-col cols="6" md="3" class="text-center mb-3">
            <div class="stat-item">
              <div class="stat-number text-info">{{ upcomingAds.length }}</div>
              <div class="stat-label">تبلیغات آتی</div>
            </div>
          </b-col>
          <b-col cols="6" md="3" class="text-center mb-3">
            <div class="stat-item">
              <div class="stat-number text-warning">{{ bannerAds.length }}</div>
              <div class="stat-label">بنر اصلی</div>
            </div>
          </b-col>
        </b-row>
      </b-card>
    </b-container>

    <!-- Ad Details Modal -->
    <b-modal
      v-model="showAdModal"
      :title="selectedAd ? selectedAd.title : ''"
      size="lg"
      hide-footer
      centered
      scrollable
      @hidden="resetSelectedAd"
    >
      <div v-if="selectedAd" class="ad-details">
        <!-- Ad Header -->
        <div class="ad-details-header mb-4">
          <b-row class="align-items-center">
            <b-col cols="8">
              <div class="ad-meta">
                <b-badge :variant="getTypeBadge(selectedAd.type)" class="ml-2">
                  {{ getTypeText(selectedAd.type) }}
                </b-badge>
                <b-badge v-if="selectedAd.isFeatured" variant="warning" class="ml-2">
                  ویژه
                </b-badge>
                <span class="text-muted ml-2">
                  <b-icon icon="eye" class="ml-1"></b-icon>
                  {{ selectedAd.views || 0 }} بازدید
                </span>
              </div>
            </b-col>
            <b-col cols="4" class="text-left">
              <b-button
                v-if="selectedAd.targetLink"
                variant="outline-primary"
                size="sm"
                :href="selectedAd.targetLink"
                target="_blank"
              >
                مشاهده لینک
                <b-icon icon="box-arrow-up-right" class="mr-1"></b-icon>
              </b-button>
            </b-col>
          </b-row>
        </div>

        <!-- Ad Image -->
        <div v-if="selectedAd.image" class="ad-details-image mb-4">
          <img
            :src="selectedAd.image"
            class="img-fluid rounded"
            :alt="selectedAd.title"
          />
        </div>

        <!-- Ad Content -->
        <div class="ad-details-content mb-4">
          <h6 class="mb-3">توضیحات کامل:</h6>
          <p class="ad-description-full">{{ selectedAd.description }}</p>
        </div>

        <!-- Ad Details -->
        <b-card class="details-card">
          <b-row>
            <b-col md="6">
              <div class="detail-item mb-3">
                <strong>تاریخ شروع:</strong>
                <span class="mr-2">{{ selectedAd.startDate }}</span>
              </div>
              <div class="detail-item mb-3">
                <strong>تاریخ پایان:</strong>
                <span class="mr-2">{{ selectedAd.endDate }}</span>
              </div>
            </b-col>
            <b-col md="6">
              <div class="detail-item mb-3">
                <strong>وضعیت:</strong>
                <b-badge :variant="getStatusBadge(selectedAd.status)" class="mr-2">
                  {{ getStatusText(selectedAd.status) }}
                </b-badge>
              </div>
              <div v-if="selectedAd.createdBy" class="detail-item mb-3">
                <strong>منتشر کننده:</strong>
                <span class="mr-2">{{ selectedAd.createdBy }}</span>
              </div>
            </b-col>
          </b-row>
        </b-card>

        <!-- Share Options -->
        <div class="share-section mt-4">
          <h6 class="mb-3">اشتراک‌گذاری:</h6>
          <div class="d-flex">
            <b-button variant="outline-secondary" size="sm" class="ml-2">
              <b-icon icon="link"></b-icon>
              کپی لینک
            </b-button>
            <b-button variant="outline-primary" size="sm" class="ml-2">
              <b-icon icon="telegram"></b-icon>
              تلگرام
            </b-button>
            <b-button variant="outline-info" size="sm" class="ml-2">
              <b-icon icon="whatsapp"></b-icon>
              واتس‌اپ
            </b-button>
          </div>
        </div>
      </div>
    </b-modal>

    <!-- Floating Action Button for Important Ads -->
    <b-button
      v-if="importantAds.length > 0"
      variant="warning"
      class="floating-action-btn"
      @click="showImportantAds"
    >
      <b-icon icon="exclamation-triangle-fill"></b-icon>
      <span class="badge-count">{{ importantAds.length }}</span>
    </b-button>

    <!-- Important Ads Modal -->
    <b-modal
      v-model="showImportantModal"
      title="تبلیغات مهم و فوری"
      hide-footer
      centered
    >
      <div v-for="ad in importantAds" :key="`important-${ad.id}`" class="mb-3">
        <b-alert variant="warning" show>
          <h6>{{ ad.title }}</h6>
          <p class="mb-2">{{ ad.description }}</p>
          <small>
            <b-icon icon="clock" class="ml-1"></b-icon>
            معتبر تا: {{ ad.endDate }}
          </small>
        </b-alert>
      </div>
    </b-modal>
  </div>
</template>

<script>
export default {
  name: "UserAdvertisements",
  data() {
    return {
      // Ads Data
      allAds: [],
      filteredAds: [],
      searchQuery: "",
      selectedType: "",
      sortBy: "newest",
      carouselSlide: 0,
      sliding: null,
      
      // Modals
      showAdModal: false,
      showImportantModal: false,
      selectedAd: null,
      
      // Filter Options
      filterTypes: [
        { value: "banner", text: "بنر" },
        { value: "video", text: "ویدیو" },
        { value: "text", text: "متنی" },
        { value: "popup", text: "پاپ‌آپ" },
        { value: "announcement", text: "اطلاعیه" }
      ],
      
      // Sort Options
      sortOptions: [
        { value: "newest", text: "جدیدترین" },
        { value: "oldest", text: "قدیمی‌ترین" },
        { value: "most_viewed", text: "پربازدیدترین" },
        { value: "ending_soon", text: "زودتر پایان می‌یابند" }
      ]
    };
  },
  computed: {
    // Active Ads
    activeAds() {
      return this.filteredAds.filter(ad => ad.status === "active");
    },
    
    // Banner Ads for Carousel
    bannerAds() {
      return this.activeAds.filter(ad => ad.type === "banner" && ad.isCarousel);
    },
    
    // Other Active Ads (non-banner)
    otherActiveAds() {
      return this.activeAds.filter(ad => ad.type !== "banner" || !ad.isCarousel);
    },
    
    // Upcoming Ads
    upcomingAds() {
      const today = new Date().toISOString().split("T")[0];
      return this.allAds.filter(ad => 
        ad.status === "upcoming" && ad.startDate > today
      );
    },
    
    // Important Ads
    importantAds() {
      return this.activeAds.filter(ad => ad.isImportant);
    },
    
    // Statistics
    totalAds() {
      return this.allAds.length;
    },
    
    activeAdsCount() {
      return this.activeAds.length;
    }
  },
  created() {
    this.loadAds();
    this.trackView();
  },
  methods: {
    // Load Ads Data
    async loadAds() {
      try {
        // Mock data - در پروژه واقعی از API استفاده کنید
        this.allAds = [
          {
            id: 1,
            title: "دوره انتخابات صندوق ذخیره فرهنگیان",
            description: "انتخابات دوره جدید صندوق ذخیره فرهنگیان آغاز شد. کلیه فرهنگیان محترم می‌توانند در این انتخابات شرکت نمایند.",
            type: "banner",
            image: "assets/img/adver/ent1.jpg?text=انتخابات+فرهنگیان",
            startDate: "1402/10/15",
            endDate: "1402/11/15",
            status: "active",
            isFeatured: true,
            isCarousel: true,
            isImportant: true,
            views: 2450,
            createdBy: "اداره انتخابات",
            targetLink: "https://example.com/election"
          },
          {
            id: 2,
            title: "مهلت ثبت‌نام کاندیداتوری",
            description: "آخرین مهلت ثبت‌نام کاندیداتوری تا پایان روز چهارشنبه می‌باشد.",
            type: "announcement",
            image: "assets/img/adver/ent1.jpg?text=مهلت+ثبت+نام",
            startDate: "1402/10/10",
            endDate: "1402/10/20",
            status: "active",
            isFeatured: false,
            isImportant: true,
            views: 1876,
            createdBy: "کمیسیون انتخابات"
          },
          {
            id: 3,
            title: "شرایط احراز صلاحیت",
            description: "شرایط و ضوابط احراز صلاحیت کاندیداها اعلام گردید.",
            type: "text",
            image: null,
            startDate: "1402/10/12",
            endDate: "1402/11/12",
            status: "active",
            isFeatured: true,
            views: 1245,
            createdBy: "کمیته نظارت"
          },
          {
            id: 4,
            title: "انتخابات آنلاین",
            description: "راهنمای استفاده از سامانه انتخابات آنلاین",
            type: "video",
            image: "assets/img/adver/ent1.jpg?text=آموزش+انتخابات",
            startDate: "1402/11/01",
            endDate: "1402/12/01",
            status: "upcoming",
            views: 0,
            createdBy: "فناوری اطلاعات"
          },
          {
            id: 5,
            title: "اعلام نتایج",
            description: "نتایج انتخابات بلافاصله پس از پایان رأی‌گیری اعلام خواهد شد.",
            type: "announcement",
            image: "assets/img/adver/ent1.jpg?text=نتایج+انتخابات",
            startDate: "1402/11/20",
            endDate: "1402/11/25",
            status: "active",
            views: 876,
            createdBy: "شورای نظارت"
          }
        ];
        
        this.filteredAds = [...this.allAds];
      } catch (error) {
        console.error("Error loading ads:", error);
      }
    },
    
    // Filter Ads
    filterAds() {
      let filtered = [...this.allAds];
      
      // Filter by search query
      if (this.searchQuery) {
        const query = this.searchQuery.toLowerCase();
        filtered = filtered.filter(ad => 
          ad.title.toLowerCase().includes(query) ||
          ad.description.toLowerCase().includes(query)
        );
      }
      
      // Filter by type
      if (this.selectedType) {
        filtered = filtered.filter(ad => ad.type === this.selectedType);
      }
      
      // Apply sorting
      this.sortAdsList(filtered);
    },
    
    // Sort Ads
    sortAds() {
      this.sortAdsList(this.filteredAds);
    },
    
    sortAdsList(list) {
      const sorted = [...list];
      
      switch (this.sortBy) {
        case "newest":
          sorted.sort((a, b) => new Date(b.startDate) - new Date(a.startDate));
          break;
        case "oldest":
          sorted.sort((a, b) => new Date(a.startDate) - new Date(b.startDate));
          break;
        case "most_viewed":
          sorted.sort((a, b) => (b.views || 0) - (a.views || 0));
          break;
        case "ending_soon":
          sorted.sort((a, b) => new Date(a.endDate) - new Date(b.endDate));
          break;
      }
      
      this.filteredAds = sorted;
    },
    
    // Clear Search
    clearSearch() {
      this.searchQuery = "";
      this.filterAds();
    },
    
    // View Ad Details
    viewAdDetails(ad) {
      this.selectedAd = ad;
      this.showAdModal = true;
      
      // Increment view count
      this.incrementViewCount(ad.id);
    },
    
    // Increment View Count
    incrementViewCount(adId) {
      const adIndex = this.allAds.findIndex(ad => ad.id === adId);
      if (adIndex !== -1) {
        this.allAds[adIndex].views = (this.allAds[adIndex].views || 0) + 1;
      }
    },
    
    // Show Important Ads
    showImportantAds() {
      this.showImportantModal = true;
    },
    
    // Carousel Events
    onSlideStart(slide) {
      this.sliding = true;
    },
    
    onSlideEnd(slide) {
      this.sliding = false;
    },
    
    // Reset Selected Ad
    resetSelectedAd() {
      this.selectedAd = null;
    },
    
    // Utility Functions
    truncateText(text, length) {
      if (text.length <= length) return text;
      return text.substring(0, length) + "...";
    },
    
    getTypeBadge(type) {
      const variants = {
        banner: "primary",
        video: "success",
        text: "info",
        popup: "warning",
        announcement: "danger"
      };
      return variants[type] || "secondary";
    },
    
    getStatusBadge(status) {
      const variants = {
        active: "success",
        inactive: "secondary",
        upcoming: "info",
        expired: "danger"
      };
      return variants[status] || "secondary";
    },
    
    getTypeText(type) {
      const typeMap = {
        banner: "بنر",
        video: "ویدیو",
        text: "متنی",
        popup: "پاپ‌آپ",
        announcement: "اطلاعیه"
      };
      return typeMap[type] || type;
    },
    
    getStatusText(status) {
      const statusMap = {
        active: "فعال",
        inactive: "غیرفعال",
        upcoming: "آتی",
        expired: "منقضی"
      };
      return statusMap[status] || status;
    },
    
    getDay(date) {
      // Extract day from date
      return date.split("/")[2];
    },
    
    getMonth(date) {
      const months = [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
      ];
      const monthIndex = parseInt(date.split("/")[1]) - 1;
      return months[monthIndex] || "";
    },
    
    // Track Page View
    trackView() {
      // Send analytics or track page view
      console.log("Advertisements page viewed");
    }
  }
};
</script>

<style scoped>
.user-advertisements-page {
  background-color: #f8f9fa;
  min-height: 100vh;
}

.ads-header {
  background: linear-gradient(135deg, #3F51B5 0%, #2196F3 100%);
  color: white;
}

.ads-container {
  padding-bottom: 50px;
}

.filter-card {
  border-radius: 12px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

/* Carousel Styles */
.carousel-image-wrapper {
  position: relative;
  cursor: pointer;
  height: 320px;
  overflow: hidden;
}

.carousel-image {
  height: 100%;
  object-fit: cover;
}

.carousel-caption-overlay {
  display: none;
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  background: linear-gradient(to top, rgba(0,0,0,0.8), transparent);
  color: white;
  padding: 20px;
  text-align: right;
}

.carousel-caption-overlay h5 {
  font-size: 1.5rem;
  margin-bottom: 5px;
}

.carousel-caption-overlay p {
  font-size: 1rem;
  margin-bottom: 5px;
  opacity: 0.9;
}

/* Ad Card */
.ad-card {
  border-radius: 10px;
  overflow: hidden;
  transition: all 0.3s ease;
  cursor: pointer;
  border: 1px solid #e0e0e0;
}

.ad-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15);
  border-color: #3F51B5;
}

.ad-card.featured-ad {
  border: 2px solid #FFC107;
}

.ad-badges {
  position: absolute;
  top: 10px;
  left: 10px;
  z-index: 1;
}

.ad-image-container {
  height: 180px;
  overflow: hidden;
  border-radius: 8px;
  background-color: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ad-image {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.ad-image-placeholder {
  color: #ccc;
}

.ad-title {
  font-size: 1.1rem;
  font-weight: 600;
  margin-bottom: 8px;
  color: #333;
}

.ad-description {
  font-size: 0.9rem;
  line-height: 1.5;
  flex-grow: 1;
}

.ad-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 10px;
  border-top: 1px solid #eee;
}

/* Upcoming Ads */
.upcoming-ad-card {
  background: linear-gradient(135deg, #f8f9fa 0%, #e3f2fd 100%);
  border: 1px solid #bbdefb;
}

.upcoming-date {
  background: #2196F3;
  color: white;
  padding: 10px;
  border-radius: 8px;
  min-width: 80px;
}

.date-day {
  font-size: 1.8rem;
  font-weight: bold;
  line-height: 1;
}

.date-month {
  font-size: 0.9rem;
  margin-top: 5px;
}

/* Section Titles */
.section-title {
  color: #333;
  padding-bottom: 8px;
  border-bottom: 2px solid #e0e0e0;
}

/* Statistics */
.statistics-card {
  border-radius: 12px;
  background: white;
}

.stat-item {
  padding: 15px;
}

.stat-number {
  font-size: 2rem;
  font-weight: bold;
  line-height: 1;
}

.stat-label {
  font-size: 0.9rem;
  color: #666;
  margin-top: 5px;
}

/* Ad Details */
.ad-details-header {
  padding-bottom: 15px;
  border-bottom: 1px solid #eee;
}

.ad-details-image {
  border-radius: 10px;
  overflow: hidden;
}

.ad-details-image img {
  width: 100%;
  max-height: 400px;
  object-fit: cover;
}

.ad-description-full {
  line-height: 1.8;
  color: #555;
  text-align: justify;
}

.details-card {
  background: #f8f9fa;
  border: 1px solid #dee2e6;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

/* Share Section */
.share-section {
  padding-top: 15px;
  border-top: 1px solid #eee;
}

/* Floating Action Button */
.floating-action-btn {
  position: fixed;
  bottom: 20px;
  left: 20px;
  width: 60px;
  height: 60px;
  border-radius: 50%;
  font-size: 1.5rem;
  box-shadow: 0 4px 12px rgba(255, 193, 7, 0.4);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}

.badge-count {
  position: absolute;
  top: -5px;
  right: -5px;
  background: #dc3545;
  color: white;
  border-radius: 50%;
  width: 24px;
  height: 24px;
  font-size: 0.8rem;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Responsive Adjustments */
@media (max-width: 768px) {
  .carousel-image-wrapper {
    height: 200px;
  }
  
  .carousel-caption-overlay h5 {
    font-size: 1.2rem;
  }
  
  .carousel-caption-overlay p {
    font-size: 0.9rem;
  }
  
  .floating-action-btn {
    bottom: 10px;
    left: 10px;
    width: 50px;
    height: 50px;
    font-size: 1.2rem;
  }
}

@media (max-width: 576px) {
  .ad-image-container {
    height: 150px;
  }
  
  .stat-number {
    font-size: 1.5rem;
  }
}
</style>