<template>
  <div class="supervisor-dashboard">
    <!-- Header -->
    <b-container fluid class="supervisor-header py-4">
      <b-row class="align-items-center">
        <b-col cols="12" md="8">
          <div class="d-flex align-items-center">
            <div class="supervisor-icon mr-3">
              <b-icon icon="shield-check"></b-icon>
            </div>
            <div>
              <h2 class="mb-1">هیأت اجرایی انتخابات</h2>
              <p class="text-muted mb-0">مدیریت و تایید مدارک کاندیداها و تبلیغات</p>
            </div>
          </div>
        </b-col>
        <b-col cols="12" md="4" class="text-left text-md-right">
          <div class="supervisor-info">
            <div class="supervisor-name">{{ currentUser.full_name }}</div>
            <div class="supervisor-role">{{ supervisor.role }}</div>
            <div class="supervisor-stats">
              <b-badge variant="info" class="mr-2">
                {{ pendingCount }} در انتظار
              </b-badge>
              <b-badge variant="success">
                {{ approvedCount }} تایید شده
              </b-badge>
            </div>
          </div>
        </b-col>
      </b-row>
    </b-container>

    <!-- Tabs Navigation -->
    <b-container class="mb-4">
      <b-card no-body>
        <b-tabs card v-model="activeTab">
          <b-tab title="مدارک کاندیداها" active>
            <div class="p-3">
              <!-- Filters -->
              <b-card class="mb-4">
                <b-row>
                  <b-col md="4">
                    <b-form-group label="فیلتر بر اساس وضعیت">
                      <b-form-select
                        v-model="docFilters.status"
                        :options="docStatusOptions"
                        @change="filterDocuments"
                      ></b-form-select>
                    </b-form-group>
                  </b-col>
                  <b-col md="4">
                    <b-form-group label="جستجو نام کاندیدا">
                      <b-input-group>
                        <template #prepend>
                          <b-input-group-text>
                            <b-icon icon="search"></b-icon>
                          </b-input-group-text>
                        </template>
                        <b-form-input
                          v-model="docFilters.search"
                          placeholder="نام کاندیدا را وارد کنید..."
                          @input="filterDocuments"
                        ></b-form-input>
                      </b-input-group>
                    </b-form-group>
                  </b-col>
                  <b-col md="4">
                    <b-form-group label="مرتب‌سازی">
                      <b-form-select
                        v-model="docFilters.sortBy"
                        :options="docSortOptions"
                        @change="filterDocuments"
                      ></b-form-select>
                    </b-form-group>
                  </b-col>
                </b-row>
              </b-card>

              <!-- Documents List -->
              <div v-if="filteredDocuments.length > 0">
                <b-row>
                  <b-col
                    v-for="candidate in filteredDocuments"
                    :key="candidate.id"
                    cols="12"
                    lg="6"
                    class="mb-4"
                  >
                    <b-card class="candidate-card">
                      <!-- Candidate Header -->
                      <div class="candidate-header mb-3">
                        <div class="d-flex align-items-center">
                          <img
                            :src="candidate.photo || '/default-avatar.png'"
                            class="candidate-avatar mr-3"
                            alt="عکس کاندیدا"
                          />
                          <div>
                            <h5 class="mb-1">{{ candidate.name }}</h5>
                            <p class="text-muted mb-1">{{ candidate.position }}</p>
                            <div class="candidate-status">
                              <b-badge :variant="getStatusVariant(candidate.status)">
                                {{ getStatusText(candidate.status) }}
                              </b-badge>
                              <small class="text-muted mr-3">
                                <b-icon icon="clock" class="ml-1"></b-icon>
                                {{ candidate.submittedAt }}
                              </small>
                            </div>
                          </div>
                        </div>
                      </div>

                      <!-- Documents List -->
                      <div class="documents-list mb-3">
                        <h6 class="mb-3">مدارک ارسال شده:</h6>
                        <div
                          v-for="doc in candidate.documents"
                          :key="doc.id"
                          class="document-item"
                        >
                          <div class="d-flex justify-content-between align-items-center">
                            <div class="document-info">
                              <b-icon :icon="getDocumentIcon(doc.type)" class="ml-2"></b-icon>
                              <span>{{ doc.name }}</span>
                              <small class="text-muted mr-2">{{ formatFileSize(doc.size) }}</small>
                            </div>
                            <div class="document-actions">
                              <b-button
                                size="sm"
                                variant="outline-primary"
                                @click="viewDocument(doc)"
                                class="mr-2"
                              >
                                <b-icon icon="eye"></b-icon>
                              </b-button>
                              <b-button
                                size="sm"
                                variant="outline-success"
                                :href="doc.url"
                                target="_blank"
                              >
                                <b-icon icon="download"></b-icon>
                              </b-button>
                            </div>
                          </div>
                          <div v-if="doc.status !== 'pending'" class="mt-2">
                            <div class="document-status" :class="`status-${doc.status}`">
                              <small>
                                <b-icon
                                  :icon="doc.status === 'approved' ? 'check-circle' : 'x-circle'"
                                  class="ml-1"
                                ></b-icon>
                                {{ doc.status === 'approved' ? 'تایید شده' : 'رد شده' }}
                                <span v-if="doc.reviewedBy"> توسط {{ doc.reviewedBy }}</span>
                                <span v-if="doc.reviewedAt"> در {{ doc.reviewedAt }}</span>
                              </small>
                              <small v-if="doc.rejectionReason" class="d-block text-danger">
                                دلیل: {{ doc.rejectionReason }}
                              </small>
                            </div>
                          </div>
                        </div>
                      </div>

                      <!-- Actions -->
                      <div class="candidate-actions">
                        <b-button
                          v-if="candidate.status === 'pending'"
                          variant="success"
                          size="sm"
                          class="mr-2"
                          @click="approveAllDocuments(candidate)"
                        >
                          <b-icon icon="check-circle" class="ml-1"></b-icon>
                          تایید همه مدارک
                        </b-button>
                        <b-button
                          v-if="candidate.status === 'pending'"
                          variant="danger"
                          size="sm"
                          class="mr-2"
                          @click="rejectCandidate(candidate)"
                        >
                          <b-icon icon="x-circle" class="ml-1"></b-icon>
                          رد صلاحیت
                        </b-button>
                        <b-button
                          variant="info"
                          size="sm"
                          @click="viewCandidateDetails(candidate)"
                        >
                          <b-icon icon="info-circle" class="ml-1"></b-icon>
                          جزئیات کامل
                        </b-button>
                      </div>
                    </b-card>
                  </b-col>
                </b-row>
              </div>

              <div v-else class="text-center py-5">
                <b-icon icon="folder-x" font-scale="4" variant="secondary"></b-icon>
                <h5 class="mt-3">مدرکی یافت نشد</h5>
                <p class="text-muted">هیچ مدرکی با فیلترهای انتخاب شده وجود ندارد.</p>
              </div>
            </div>
          </b-tab>

          <b-tab title="تبلیغات">
            <div class="p-3">
              <!-- Filters -->
              <b-card class="mb-4">
                <b-row>
                  <b-col md="4">
                    <b-form-group label="فیلتر بر اساس وضعیت">
                      <b-form-select
                        v-model="adFilters.status"
                        :options="adStatusOptions"
                        @change="filterAds"
                      ></b-form-select>
                    </b-form-group>
                  </b-col>
                  <b-col md="4">
                    <b-form-group label="نوع تبلیغ">
                      <b-form-select
                        v-model="adFilters.type"
                        :options="adTypeOptions"
                        @change="filterAds"
                      ></b-form-select>
                    </b-form-group>
                  </b-col>
                  <b-col md="4">
                    <b-form-group label="جستجو">
                      <b-input-group>
                        <template #prepend>
                          <b-input-group-text>
                            <b-icon icon="search"></b-icon>
                          </b-input-group-text>
                        </template>
                        <b-form-input
                          v-model="adFilters.search"
                          placeholder="جستجو در تبلیغات..."
                          @input="filterAds"
                        ></b-form-input>
                      </b-input-group>
                    </b-form-group>
                  </b-col>
                </b-row>
              </b-card>

              <!-- Ads List -->
              <div class="table-responsive">
                <b-table
                  :items="filteredAds"
                  :fields="adFields"
                  striped
                  hover
                  class="text-right"
                >
                  <!-- Preview Column -->
                  <template #cell(preview)="data">
                    <div class="ad-preview">
                      <img
                        v-if="data.item.image"
                        :src="data.item.image"
                        class="ad-thumbnail"
                        :alt="data.item.title"
                        @click="viewAd(data.item)"
                      />
                      <div v-else class="ad-thumbnail placeholder">
                        <b-icon icon="image"></b-icon>
                      </div>
                    </div>
                  </template>

                  <!-- Status Column -->
                  <template #cell(status)="data">
                    <b-badge :variant="getAdStatusVariant(data.item.status)">
                      {{ getAdStatusText(data.item.status) }}
                    </b-badge>
                  </template>

                  <!-- Actions Column -->
                  <template #cell(actions)="data">
                    <b-button-group size="sm">
                      <b-button
                        variant="outline-info"
                        @click="viewAd(data.item)"
                        title="مشاهده"
                      >
                        <b-icon icon="eye"></b-icon>
                      </b-button>
                      <b-button
                        v-if="data.item.status === 'pending'"
                        variant="outline-success"
                        @click="approveAd(data.item)"
                        title="تایید"
                      >
                        <b-icon icon="check"></b-icon>
                      </b-button>
                      <b-button
                        v-if="data.item.status === 'pending'"
                        variant="outline-danger"
                        @click="rejectAd(data.item)"
                        title="رد"
                      >
                        <b-icon icon="x"></b-icon>
                      </b-button>
                      <b-button
                        variant="outline-warning"
                        @click="editAd(data.item)"
                        title="ویرایش"
                      >
                        <b-icon icon="pencil"></b-icon>
                      </b-button>
                    </b-button-group>
                  </template>
                </b-table>
              </div>
            </div>
          </b-tab>

          <b-tab title="آمار و گزارشات">
            <div class="p-3">
              <!-- Statistics Cards -->
              <b-row class="mb-4">
                <b-col md="3">
                  <b-card class="stat-card text-center">
                    <div class="stat-icon total">
                      <b-icon icon="people-fill"></b-icon>
                    </div>
                    <div class="stat-number">{{ stats.totalCandidates }}</div>
                    <div class="stat-label">کل کاندیداها</div>
                  </b-card>
                </b-col>
                <b-col md="3">
                  <b-card class="stat-card text-center">
                    <div class="stat-icon pending">
                      <b-icon icon="clock-fill"></b-icon>
                    </div>
                    <div class="stat-number">{{ stats.pendingDocuments }}</div>
                    <div class="stat-label">مدارک در انتظار</div>
                  </b-card>
                </b-col>
                <b-col md="3">
                  <b-card class="stat-card text-center">
                    <div class="stat-icon approved">
                      <b-icon icon="check-circle-fill"></b-icon>
                    </div>
                    <div class="stat-number">{{ stats.approvedCandidates }}</div>
                    <div class="stat-label">تایید شده‌ها</div>
                  </b-card>
                </b-col>
                <b-col md="3">
                  <b-card class="stat-card text-center">
                    <div class="stat-icon rejected">
                      <b-icon icon="x-circle-fill"></b-icon>
                    </div>
                    <div class="stat-number">{{ stats.rejectedCandidates }}</div>
                    <div class="stat-label">رد صلاحیت شده‌ها</div>
                  </b-card>
                </b-col>
              </b-row>

              <!-- Charts -->
              <b-row class="mb-4">
                <b-col md="6">
                  <b-card>
                    <h5 class="mb-3">وضعیت مدارک کاندیداها</h5>
                    <div class="chart-container">
                      <canvas ref="documentsChart"></canvas>
                    </div>
                  </b-card>
                </b-col>
                <b-col md="6">
                  <b-card>
                    <h5 class="mb-3">وضعیت تبلیغات</h5>
                    <div class="chart-container">
                      <canvas ref="adsChart"></canvas>
                    </div>
                  </b-card>
                </b-col>
              </b-row>

              <!-- Recent Activities -->
              <b-card>
                <h5 class="mb-3">فعالیت‌های اخیر</h5>
                <div class="activities-list">
                  <div
                    v-for="activity in recentActivities"
                    :key="activity.id"
                    class="activity-item"
                  >
                    <div class="activity-icon">
                      <b-icon :icon="getActivityIcon(activity.type)"></b-icon>
                    </div>
                    <div class="activity-content">
                      <div class="activity-text">{{ activity.text }}</div>
                      <div class="activity-time">{{ activity.time }}</div>
                    </div>
                    <div class="activity-user">
                      <small>{{ activity.user }}</small>
                    </div>
                  </div>
                </div>
              </b-card>
            </div>
          </b-tab>
        </b-tabs>
      </b-card>
    </b-container>

    <!-- Document Viewer Modal -->
    <b-modal
      v-model="showDocumentModal"
      :title="`مشاهده مدرک - ${selectedDocument?.name}`"
      size="xl"
      hide-footer
      centered
      scrollable
    >
      <div v-if="selectedDocument" class="document-viewer">
        <!-- Image Viewer -->
        <div v-if="isImageFile(selectedDocument.url)" class="image-viewer">
          <img :src="selectedDocument.url" class="img-fluid" :alt="selectedDocument.name" />
        </div>

        <!-- PDF Viewer -->
        <div v-else-if="isPdfFile(selectedDocument.url)" class="pdf-viewer">
          <iframe
            :src="selectedDocument.url"
            class="pdf-frame"
            :title="selectedDocument.name"
          ></iframe>
        </div>

        <!-- Other Files -->
        <div v-else class="file-info">
          <div class="text-center py-5">
            <b-icon icon="file-earmark" font-scale="4" variant="secondary"></b-icon>
            <h5 class="mt-3">{{ selectedDocument.name }}</h5>
            <p class="text-muted">برای مشاهده این فایل، آن را دانلود کنید</p>
            <b-button
              variant="primary"
              :href="selectedDocument.url"
              target="_blank"
              download
            >
              <b-icon icon="download" class="ml-1"></b-icon>
              دانلود فایل
            </b-button>
          </div>
        </div>

        <!-- Document Details -->
        <div class="document-details mt-4">
          <b-row>
            <b-col md="6">
              <div class="detail-item">
                <strong>نام فایل:</strong>
                <span>{{ selectedDocument.name }}</span>
              </div>
              <div class="detail-item">
                <strong>نوع فایل:</strong>
                <span>{{ selectedDocument.type }}</span>
              </div>
              <div class="detail-item">
                <strong>حجم فایل:</strong>
                <span>{{ formatFileSize(selectedDocument.size) }}</span>
              </div>
            </b-col>
            <b-col md="6">
              <div class="detail-item">
                <strong>تاریخ آپلود:</strong>
                <span>{{ selectedDocument.uploadedAt }}</span>
              </div>
              <div class="detail-item">
                <strong>وضعیت:</strong>
                <b-badge :variant="getStatusVariant(selectedDocument.status)">
                  {{ getStatusText(selectedDocument.status) }}
                </b-badge>
              </div>
              <div v-if="selectedDocument.reviewedBy" class="detail-item">
                <strong>بررسی کننده:</strong>
                <span>{{ selectedDocument.reviewedBy }}</span>
              </div>
            </b-col>
          </b-row>
        </div>

        <!-- Review Form -->
        <div v-if="selectedDocument.status === 'pending'" class="review-form mt-4">
          <h6 class="mb-3">بررسی مدرک</h6>
          <b-form @submit.prevent="submitReview">
            <b-form-group label="نظر بررسی" label-for="review-comment">
              <b-form-textarea
                id="review-comment"
                v-model="reviewComment"
                rows="3"
                placeholder="نظر خود را وارد کنید..."
              ></b-form-textarea>
            </b-form-group>

            <div class="text-center">
              <b-button
                type="submit"
                variant="success"
                class="mr-3"
                @click="approveDocument"
              >
                <b-icon icon="check" class="ml-1"></b-icon>
                تایید مدرک
              </b-button>
              <b-button
                variant="danger"
                @click="rejectDocument"
                :disabled="!reviewComment"
              >
                <b-icon icon="x" class="ml-1"></b-icon>
                رد مدرک
              </b-button>
            </div>
          </b-form>
        </div>
      </div>
    </b-modal>

    <!-- Candidate Details Modal -->
    <b-modal
      v-model="showCandidateModal"
      :title="`جزئیات کاندیدا - ${selectedCandidate?.name}`"
      size="lg"
      hide-footer
      centered
      scrollable
    >
      <div v-if="selectedCandidate" class="candidate-details">
        <!-- Basic Info -->
        <div class="basic-info mb-4">
          <b-row class="align-items-center">
            <b-col md="4" class="text-center">
              <img
                :src="selectedCandidate.photo || '/default-avatar.png'"
                class="candidate-photo"
                alt="عکس کاندیدا"
              />
            </b-col>
            <b-col md="8">
              <h4>{{ selectedCandidate.name }}</h4>
              <p class="text-muted">{{ selectedCandidate.position }}</p>
              <div class="candidate-meta">
                <div class="meta-item">
                  <b-icon icon="geo-alt" class="ml-1"></b-icon>
                  {{ selectedCandidate.city }}
                </div>
                <div class="meta-item">
                  <b-icon icon="calendar" class="ml-1"></b-icon>
                  {{ selectedCandidate.birthDate }}
                </div>
                <div class="meta-item">
                  <b-icon icon="briefcase" class="ml-1"></b-icon>
                  {{ selectedCandidate.experience }} سال سابقه
                </div>
              </div>
            </b-col>
          </b-row>
        </div>

        <!-- Documents Summary -->
        <div class="documents-summary mb-4">
          <h6 class="mb-3">خلاصه مدارک:</h6>
          <b-row>
            <b-col md="4">
              <div class="summary-item total">
                <div class="summary-number">{{ selectedCandidate.documents.length }}</div>
                <div class="summary-label">کل مدارک</div>
              </div>
            </b-col>
            <b-col md="4">
              <div class="summary-item approved">
                <div class="summary-number">{{ getApprovedDocsCount(selectedCandidate) }}</div>
                <div class="summary-label">تایید شده</div>
              </div>
            </b-col>
            <b-col md="4">
              <div class="summary-item pending">
                <div class="summary-number">{{ getPendingDocsCount(selectedCandidate) }}</div>
                <div class="summary-label">در انتظار</div>
              </div>
            </b-col>
          </b-row>
        </div>

        <!-- Biography -->
        <div class="biography mb-4">
          <h6 class="mb-3">زندگینامه:</h6>
          <p class="bio-text">{{ selectedCandidate.biography }}</p>
        </div>

        <!-- Program -->
        <div class="program mb-4">
          <h6 class="mb-3">برنامه انتخابی:</h6>
          <ul class="program-list">
            <li v-for="(item, index) in selectedCandidate.program" :key="index">
              {{ item }}
            </li>
          </ul>
        </div>

        <!-- Final Decision -->
        <div v-if="selectedCandidate.status === 'pending'" class="final-decision">
          <b-alert variant="warning" show>
            <h6 class="alert-heading">تصمیم نهایی</h6>
            <p>پس از بررسی تمام مدارک، تصمیم نهایی را در مورد صلاحیت این کاندیدا بگیرید.</p>
            
            <b-form-group label="نظر نهایی" label-for="final-comment">
              <b-form-textarea
                id="final-comment"
                v-model="finalComment"
                rows="2"
                placeholder="نظر نهایی را وارد کنید..."
              ></b-form-textarea>
            </b-form-group>

            <div class="text-center mt-3">
              <b-button
                variant="success"
                class="mr-3"
                @click="approveCandidate"
                :disabled="!finalComment"
              >
                <b-icon icon="check-circle" class="ml-1"></b-icon>
                تایید صلاحیت
              </b-button>
              <b-button
                variant="danger"
                @click="rejectCandidate(selectedCandidate)"
                :disabled="!finalComment"
              >
                <b-icon icon="x-circle" class="ml-1"></b-icon>
                رد صلاحیت
              </b-button>
            </div>
          </b-alert>
        </div>
      </div>
    </b-modal>

    <!-- Ad Details Modal -->
    <b-modal
      v-model="showAdModal"
      :title="`تبلیغ - ${selectedAd?.title}`"
      size="lg"
      hide-footer
      centered
      scrollable
    >
      <div v-if="selectedAd" class="ad-details">
        <!-- Ad Content -->
        <div class="ad-content mb-4">
          <div v-if="selectedAd.image" class="ad-image mb-3">
            <img :src="selectedAd.image" class="img-fluid" alt="تصویر تبلیغ" />
          </div>
          <h5>{{ selectedAd.title }}</h5>
          <p class="ad-description">{{ selectedAd.description }}</p>
        </div>

        <!-- Ad Info -->
        <b-row class="mb-4">
          <b-col md="6">
            <div class="info-item">
              <strong>نوع تبلیغ:</strong>
              <span>{{ getAdTypeText(selectedAd.type) }}</span>
            </div>
            <div class="info-item">
              <strong>تاریخ شروع:</strong>
              <span>{{ selectedAd.startDate }}</span>
            </div>
            <div class="info-item">
              <strong>تاریخ پایان:</strong>
              <span>{{ selectedAd.endDate }}</span>
            </div>
          </b-col>
          <b-col md="6">
            <div class="info-item">
              <strong>وضعیت:</strong>
              <b-badge :variant="getAdStatusVariant(selectedAd.status)">
                {{ getAdStatusText(selectedAd.status) }}
              </b-badge>
            </div>
            <div class="info-item">
              <strong>ایجاد کننده:</strong>
              <span>{{ selectedAd.createdBy }}</span>
            </div>
            <div class="info-item">
              <strong>تاریخ ایجاد:</strong>
              <span>{{ selectedAd.createdAt }}</span>
            </div>
          </b-col>
        </b-row>

        <!-- Ad Review -->
        <div v-if="selectedAd.status === 'pending'" class="ad-review">
          <h6 class="mb-3">بررسی تبلیغ</h6>
          <b-form @submit.prevent="reviewAd">
            <b-form-group label="نظر بررسی" label-for="ad-review-comment">
              <b-form-textarea
                id="ad-review-comment"
                v-model="adReviewComment"
                rows="3"
                placeholder="نظر خود را در مورد این تبلیغ وارد کنید..."
              ></b-form-textarea>
            </b-form-group>

            <div class="text-center">
              <b-button
                type="submit"
                variant="success"
                class="mr-3"
                @click="approveSelectedAd"
              >
                <b-icon icon="check" class="ml-1"></b-icon>
                تایید تبلیغ
              </b-button>
              <b-button
                variant="danger"
                @click="rejectSelectedAd"
                :disabled="!adReviewComment"
              >
                <b-icon icon="x" class="ml-1"></b-icon>
                رد تبلیغ
              </b-button>
            </div>
          </b-form>
        </div>

        <!-- Previous Reviews -->
        <div v-if="selectedAd.reviews && selectedAd.reviews.length > 0" class="previous-reviews mt-4">
          <h6 class="mb-3">بررسی‌های قبلی</h6>
          <div v-for="review in selectedAd.reviews" :key="review.id" class="review-item">
            <div class="review-header">
              <strong>{{ review.reviewer }}</strong>
              <small class="text-muted">{{ review.date }}</small>
              <b-badge :variant="review.status === 'approved' ? 'success' : 'danger'">
                {{ review.status === 'approved' ? 'تایید' : 'رد' }}
              </b-badge>
            </div>
            <div class="review-comment">{{ review.comment }}</div>
          </div>
        </div>
      </div>
    </b-modal>

    <!-- Bulk Actions -->
    <div class="bulk-actions" v-if="selectedDocuments.length > 0">
      <b-card class="bulk-card">
        <div class="d-flex justify-content-between align-items-center">
          <div>
            <strong>{{ selectedDocuments.length }} مدرک انتخاب شده</strong>
          </div>
          <div>
            <b-button
              variant="success"
              size="sm"
              class="mr-2"
              @click="bulkApprove"
            >
              <b-icon icon="check-circle" class="ml-1"></b-icon>
              تایید انتخابی
            </b-button>
            <b-button
              variant="danger"
              size="sm"
              class="mr-2"
              @click="bulkReject"
            >
              <b-icon icon="x-circle" class="ml-1"></b-icon>
              رد انتخابی
            </b-button>
            <b-button
              variant="secondary"
              size="sm"
              @click="clearSelection"
            >
              <b-icon icon="x" class="ml-1"></b-icon>
              لغو انتخاب
            </b-button>
          </div>
        </div>
      </b-card>
    </div>
  </div>
</template>

<script>
import Chart from 'chart.js';
import {  isMobile } from "../../utils";
import { mapGetters, mapActions, mapMutations } from "vuex";
export default {
  name: 'ExecutiveDashboard',
  data() {
    return {
      isMobile,
      activeTab: 0,
      supervisor: {
        role: 'عضو هیأت اجرایی',
        department: 'کمیسیون نظارت بر انتخابات'
      },
      
      // Documents Data
      candidates: [
        {
          id: 1,
          name: 'دکتر محمدرضا احمدی',
          position: 'استاد دانشگاه - علوم تربیتی',
          photo: 'assets/img/avatars/imagen1.png?text=MA',
          city: 'تهران',
          birthDate: '۱۳۵۰/۰۵/۱۵',
          experience: 15,
          status: 'pending',
          submittedAt: '۱۴۰۲/۱۱/۱۰',
          biography: 'دارای ۱۵ سال سابقه تدریس و مدیریت در دانشگاه‌های معتبر کشور',
          program: [
            'شفاف‌سازی مالی صندوق',
            'افزایش بازدهی سرمایه‌گذاری‌ها',
            'ایجاد بیمه تکمیلی برای فرهنگیان'
          ],
          documents: [
            {
              id: 101,
              name: 'مدرک تحصیلی دکتری.pdf',
              type: 'degree',
              url: 'https://example.com/diploma.pdf',
              size: 2048576,
              status: 'pending',
              uploadedAt: '۱۴۰۲/۱۱/۱۰'
            },
            {
              id: 102,
              name: 'عکس پرسنلی.jpg',
              type: 'photo',
              url: 'assets/img/avatars/imagen1.png?text=Photo',
              size: 512000,
              status: 'approved',
              reviewedBy: 'دکتر موسوی',
              reviewedAt: '۱۴۰۲/۱۱/۱۱'
            },
            {
              id: 103,
              name: 'گواهی عدم اعتیاد.pdf',
              type: 'no_addiction',
              url: 'https://example.com/no_addiction.pdf',
              size: 1024000,
              status: 'pending',
              uploadedAt: '۱۴۰۲/۱۱/۱۰'
            }
          ]
        },
        {
          id: 2,
          name: 'مهندس سید علی حسینی',
          position: 'مدیر آموزش و پرورش منطقه ۵',
          photo: 'assets/img/avatars/imagen1.png?text=SH',
          city: 'تهران',
          birthDate: '۱۳۵۵/۰۸/۲۰',
          experience: 12,
          status: 'approved',
          submittedAt: '۱۴۰۲/۱۱/۰۹',
          documents: [
            {
              id: 201,
              name: 'مدرک کارشناسی ارشد.pdf',
              type: 'degree',
              url: 'https://example.com/masters.pdf',
              size: 1800000,
              status: 'approved',
              reviewedBy: 'دکتر موسوی',
              reviewedAt: '۱۴۰۲/۱۱/۱۰'
            }
          ]
        },
        {
          id: 3,
          name: 'دکتر فاطمه کریمی',
          position: 'معاون پژوهشی - پژوهشگاه مطالعات',
          photo: 'assets/img/avatars/imagen1.png?text=FK',
          city: 'مشهد',
          birthDate: '۱۳۶۰/۰۳/۱۰',
          experience: 10,
          status: 'rejected',
          submittedAt: '۱۴۰۲/۱۱/۰۸',
          documents: [
            {
              id: 301,
              name: 'مدرک دکتری.pdf',
              type: 'degree',
              url: 'https://example.com/phd.pdf',
              size: 2200000,
              status: 'rejected',
              reviewedBy: 'دکتر موسوی',
              reviewedAt: '۱۴۰۲/۱۱/۰۹',
              rejectionReason: 'مدرک معتبر نیست'
            }
          ]
        }
      ],
      
      // Ads Data
      advertisements: [
        {
          id: 1,
          title: 'دوره انتخابات صندوق ذخیره فرهنگیان',
          description: 'انتخابات دوره جدید صندوق ذخیره فرهنگیان آغاز شد',
          image: 'assets/img/avatars/imagen1.png?text=Election',
          type: 'banner',
          status: 'pending',
          startDate: '۱۴۰۲/۱۱/۱۵',
          endDate: '۱۴۰۲/۱۱/۲۰',
          createdBy: 'اداره انتخابات',
          createdAt: '۱۴۰۲/۱۱/۱۰'
        },
        {
          id: 2,
          title: 'مهلت ثبت‌نام کاندیداتوری',
          description: 'آخرین مهلت ثبت‌نام کاندیداتوری',
          type: 'announcement',
          status: 'approved',
          startDate: '۱۴۰۲/۱۱/۱۲',
          endDate: '۱۴۰۲/۱۱/۱۵',
          createdBy: 'کمیسیون انتخابات',
          createdAt: '۱۴۰۲/۱۱/۰۸',
          reviews: [
            {
              id: 1,
              reviewer: 'دکتر موسوی',
              date: '۱۴۰۲/۱۱/۰۹',
              status: 'approved',
              comment: 'تبلیغ مناسب و مفید است'
            }
          ]
        },
        {
          id: 3,
          title: 'شرایط احراز صلاحیت',
          description: 'شرایط و ضوابط احراز صلاحیت کاندیداها',
          image: 'assets/img/avatars/imagen1.png?text=Conditions',
          type: 'info',
          status: 'rejected',
          startDate: '۱۴۰۲/۱۱/۱۰',
          endDate: '۱۴۰۲/۱۱/۱۵',
          createdBy: 'کمیته نظارت',
          createdAt: '۱۴۰۲/۱۱/۰۷',
          reviews: [
            {
              id: 2,
              reviewer: 'دکتر موسوی',
              date: '۱۴۰۲/۱۱/۰۸',
              status: 'rejected',
              comment: 'نیاز به اصلاح متن دارد'
            }
          ]
        }
      ],
      
      // Filters
      docFilters: {
        status: 'all',
        search: '',
        sortBy: 'newest'
      },
      adFilters: {
        status: 'all',
        type: 'all',
        search: ''
      },
      
      // Options
      docStatusOptions: [
        { value: 'all', text: 'همه وضعیت‌ها' },
        { value: 'pending', text: 'در انتظار' },
        { value: 'approved', text: 'تایید شده' },
        { value: 'rejected', text: 'رد شده' }
      ],
      docSortOptions: [
        { value: 'newest', text: 'جدیدترین' },
        { value: 'oldest', text: 'قدیمی‌ترین' },
        { value: 'name', text: 'نام الفبایی' }
      ],
      adStatusOptions: [
        { value: 'all', text: 'همه وضعیت‌ها' },
        { value: 'pending', text: 'در انتظار' },
        { value: 'approved', text: 'تایید شده' },
        { value: 'rejected', text: 'رد شده' },
        { value: 'active', text: 'فعال' },
        { value: 'expired', text: 'منقضی' }
      ],
      adTypeOptions: [
        { value: 'all', text: 'همه انواع' },
        { value: 'banner', text: 'بنر' },
        { value: 'announcement', text: 'اطلاعیه' },
        { value: 'info', text: 'اطلاع رسانی' },
        { value: 'promotional', text: 'تبلیغاتی' }
      ],
      
      // Table Fields
      adFields: [
        { key: 'preview', label: 'پیش‌نمایش', sortable: false },
        { key: 'title', label: 'عنوان', sortable: true },
        { key: 'type', label: 'نوع', sortable: true },
        { key: 'status', label: 'وضعیت', sortable: true },
        { key: 'startDate', label: 'تاریخ شروع', sortable: true },
        { key: 'endDate', label: 'تاریخ پایان', sortable: true },
        { key: 'createdBy', label: 'ایجاد کننده', sortable: true },
        { key: 'actions', label: 'عملیات', sortable: false }
      ],
      
      // Statistics
      stats: {
        totalCandidates: 0,
        pendingDocuments: 0,
        approvedCandidates: 0,
        rejectedCandidates: 0,
        pendingAds: 0,
        approvedAds: 0,
        rejectedAds: 0
      },
      
      // Recent Activities
      recentActivities: [
        {
          id: 1,
          type: 'approve',
          text: 'مدارک دکتر احمدی تایید شد',
          time: '۱۰ دقیقه پیش',
          user: 'دکتر موسوی'
        },
        {
          id: 2,
          type: 'reject',
          text: 'تبلیغ شرایط صلاحیت رد شد',
          time: '۱ ساعت پیش',
          user: 'دکتر موسوی'
        },
        {
          id: 3,
          type: 'upload',
          text: 'کاندیدای جدید ثبت‌نام کرد',
          time: '۲ ساعت پیش',
          user: 'سیستم'
        },
        {
          id: 4,
          type: 'approve',
          text: 'تبلیغ مهلت ثبت‌نام تایید شد',
          time: '۳ ساعت پیش',
          user: 'دکتر موسوی'
        }
      ],
      
      // Modals
      showDocumentModal: false,
      showCandidateModal: false,
      showAdModal: false,
      
      // Selected Items
      selectedDocument: null,
      selectedCandidate: null,
      selectedAd: null,
      selectedDocuments: [],
      
      // Review Data
      reviewComment: '',
      finalComment: '',
      adReviewComment: '',
      
      // Charts
      documentsChart: null,
      adsChart: null
    };
  },
  computed: {
    ...mapGetters(["currentUser"]),
    filteredDocuments() {
      let filtered = [...this.candidates];
      
      // Filter by status
      if (this.docFilters.status !== 'all') {
        filtered = filtered.filter(candidate => candidate.status === this.docFilters.status);
      }
      
      // Filter by search
      if (this.docFilters.search) {
        const search = this.docFilters.search.toLowerCase();
        filtered = filtered.filter(candidate => 
          candidate.name.toLowerCase().includes(search) ||
          candidate.position.toLowerCase().includes(search)
        );
      }
      
      // Sort
      switch (this.docFilters.sortBy) {
        case 'newest':
          filtered.sort((a, b) => new Date(b.submittedAt) - new Date(a.submittedAt));
          break;
        case 'oldest':
          filtered.sort((a, b) => new Date(a.submittedAt) - new Date(b.submittedAt));
          break;
        case 'name':
          filtered.sort((a, b) => a.name.localeCompare(b.name));
          break;
      }
      
      return filtered;
    },
    
    filteredAds() {
      let filtered = [...this.advertisements];
      
      // Filter by status
      if (this.adFilters.status !== 'all') {
        filtered = filtered.filter(ad => ad.status === this.adFilters.status);
      }
      
      // Filter by type
      if (this.adFilters.type !== 'all') {
        filtered = filtered.filter(ad => ad.type === this.adFilters.type);
      }
      
      // Filter by search
      if (this.adFilters.search) {
        const search = this.adFilters.search.toLowerCase();
        filtered = filtered.filter(ad => 
          ad.title.toLowerCase().includes(search) ||
          ad.description.toLowerCase().includes(search)
        );
      }
      
      return filtered;
    },
    
    pendingCount() {
      return this.candidates.filter(c => c.status === 'pending').length +
             this.advertisements.filter(a => a.status === 'pending').length;
    },
    
    approvedCount() {
      return this.candidates.filter(c => c.status === 'approved').length +
             this.advertisements.filter(a => a.status === 'approved').length;
    }
  },
  mounted() {
    this.calculateStats();
    this.initializeCharts();
  },
  methods: {
    // Helper Methods
    getStatusVariant(status) {
      const variants = {
        pending: 'warning',
        approved: 'success',
        rejected: 'danger'
      };
      return variants[status] || 'secondary';
    },
    
    getStatusText(status) {
      const texts = {
        pending: 'در انتظار',
        approved: 'تایید شده',
        rejected: 'رد شده'
      };
      return texts[status] || status;
    },
    
    getAdStatusVariant(status) {
      const variants = {
        pending: 'warning',
        approved: 'success',
        rejected: 'danger',
        active: 'info',
        expired: 'secondary'
      };
      return variants[status] || 'secondary';
    },
    
    getAdStatusText(status) {
      const texts = {
        pending: 'در انتظار',
        approved: 'تایید شده',
        rejected: 'رد شده',
        active: 'فعال',
        expired: 'منقضی'
      };
      return texts[status] || status;
    },
    
    getAdTypeText(type) {
      const texts = {
        banner: 'بنر',
        announcement: 'اطلاعیه',
        info: 'اطلاع رسانی',
        promotional: 'تبلیغاتی'
      };
      return texts[type] || type;
    },
    
    getDocumentIcon(type) {
      const icons = {
        degree: 'file-earmark-text',
        photo: 'image',
        no_addiction: 'file-medical',
        id_card: 'credit-card',
        experience: 'briefcase'
      };
      return icons[type] || 'file-earmark';
    },
    
    getActivityIcon(type) {
      const icons = {
        approve: 'check-circle',
        reject: 'x-circle',
        upload: 'cloud-upload',
        edit: 'pencil',
        delete: 'trash'
      };
      return icons[type] || 'info-circle';
    },
    
    formatFileSize(bytes) {
      if (bytes === 0) return '0 بایت';
      const k = 1024;
      const sizes = ['بایت', 'کیلوبایت', 'مگابایت', 'گیگابایت'];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    },
    
    isImageFile(url) {
      return /\.(jpg|jpeg|png|gif|bmp|webp)$/i.test(url);
    },
    
    isPdfFile(url) {
      return /\.pdf$/i.test(url);
    },
    
    // Document Methods
    viewDocument(doc) {
      this.selectedDocument = doc;
      this.reviewComment = '';
      this.showDocumentModal = true;
    },
    
    approveDocument() {
      if (this.selectedDocument) {
        this.selectedDocument.status = 'approved';
        this.selectedDocument.reviewedBy = this.supervisor.name;
        this.selectedDocument.reviewedAt = this.getCurrentDate();
        
        this.$bvToast.toast('مدرک با موفقیت تایید شد', {
          title: 'تایید موفق',
          variant: 'success',
          solid: true
        });
        
        this.updateCandidateStatus();
        this.calculateStats();
        this.showDocumentModal = false;
      }
    },
    
    rejectDocument() {
      if (this.selectedDocument && this.reviewComment) {
        this.selectedDocument.status = 'rejected';
        this.selectedDocument.reviewedBy = this.supervisor.name;
        this.selectedDocument.reviewedAt = this.getCurrentDate();
        this.selectedDocument.rejectionReason = this.reviewComment;
        
        this.$bvToast.toast('مدرک رد شد', {
          title: 'رد مدرک',
          variant: 'warning',
          solid: true
        });
        
        this.updateCandidateStatus();
        this.calculateStats();
        this.showDocumentModal = false;
      }
    },
    
    updateCandidateStatus() {
      // Find candidate
      const candidate = this.candidates.find(c => 
        c.documents.some(d => d.id === this.selectedDocument.id)
      );
      
      if (candidate) {
        // Check if all documents are approved
        const allApproved = candidate.documents.every(d => d.status === 'approved');
        const anyRejected = candidate.documents.some(d => d.status === 'rejected');
        
        if (allApproved) {
          candidate.status = 'approved';
        } else if (anyRejected) {
          candidate.status = 'rejected';
        }
      }
    },
    
    // Candidate Methods
    viewCandidateDetails(candidate) {
      this.selectedCandidate = candidate;
      this.finalComment = '';
      this.showCandidateModal = true;
    },
    
    getApprovedDocsCount(candidate) {
      return candidate.documents.filter(d => d.status === 'approved').length;
    },
    
    getPendingDocsCount(candidate) {
      return candidate.documents.filter(d => d.status === 'pending').length;
    },
    
    approveAllDocuments(candidate) {
      candidate.documents.forEach(doc => {
        if (doc.status === 'pending') {
          doc.status = 'approved';
          doc.reviewedBy = this.supervisor.name;
          doc.reviewedAt = this.getCurrentDate();
        }
      });
      
      candidate.status = 'approved';
      
      this.$bvToast.toast('همه مدارک کاندیدا تایید شد', {
        title: 'تایید موفق',
        variant: 'success',
        solid: true
      });
      
      this.calculateStats();
    },
    
    approveCandidate() {
      if (this.selectedCandidate && this.finalComment) {
        this.selectedCandidate.status = 'approved';
        this.selectedCandidate.documents.forEach(doc => {
          if (doc.status === 'pending') {
            doc.status = 'approved';
            doc.reviewedBy = this.supervisor.name;
            doc.reviewedAt = this.getCurrentDate();
          }
        });
        
        // Add to activities
        this.recentActivities.unshift({
          id: Date.now(),
          type: 'approve',
          text: `صلاحیت ${this.selectedCandidate.name} تایید شد`,
          time: 'همین حالا',
          user: this.supervisor.name
        });
        
        this.$bvToast.toaste('صلاحیت کاندیدا تایید شد', {
          title: 'تایید صلاحیت',
          variant: 'success',
          solid: true
        });
        
        this.calculateStats();
        this.showCandidateModal = false;
      }
    },
    
    rejectCandidate(candidate) {
      this.$bvModal.msgBoxConfirm('آیا از رد صلاحیت این کاندیدا اطمینان دارید؟', {
        title: 'تایید رد صلاحیت',
        size: 'md',
        buttonSize: 'sm',
        okVariant: 'danger',
        okTitle: 'بله، رد کن',
        cancelTitle: 'لغو',
        hideHeaderClose: false,
        centered: true
      }).then(value => {
        if (value) {
          candidate.status = 'rejected';
          candidate.documents.forEach(doc => {
            if (doc.status === 'pending') {
              doc.status = 'rejected';
              doc.reviewedBy = this.supervisor.name;
              doc.reviewedAt = this.getCurrentDate();
              doc.rejectionReason = 'رد صلاحیت کلی کاندیدا';
            }
          });
          
          // Add to activities
          this.recentActivities.unshift({
            id: Date.now(),
            type: 'reject',
            text: `صلاحیت ${candidate.name} رد شد`,
            time: 'همین حالا',
            user: this.supervisor.name
          });
          
          this.$bvToast.toast('صلاحیت کاندیدا رد شد', {
            title: 'رد صلاحیت',
            variant: 'warning',
            solid: true
          });
          
          this.calculateStats();
          if (this.showCandidateModal) {
            this.showCandidateModal = false;
          }
        }
      });
    },
    
    // Ad Methods
    viewAd(ad) {
      this.selectedAd = ad;
      this.adReviewComment = '';
      this.showAdModal = true;
    },
    
    approveAd(ad) {
      this.selectedAd = ad;
      this.approveSelectedAd();
    },
    
    rejectAd(ad) {
      this.selectedAd = ad;
      this.rejectSelectedAd();
    },
    
    editAd(ad) {
      // In real app, navigate to edit page
      this.$router.push(`/ads/edit/${ad.id}`);
    },
    
    approveSelectedAd() {
      if (this.selectedAd) {
        this.selectedAd.status = 'approved';
        
        if (!this.selectedAd.reviews) {
          this.selectedAd.reviews = [];
        }
        
        this.selectedAd.reviews.push({
          id: Date.now(),
          reviewer: this.supervisor.name,
          date: this.getCurrentDate(),
          status: 'approved',
          comment: this.adReviewComment || 'تبلیغ مناسب تشخیص داده شد'
        });
        
        // Add to activities
        this.recentActivities.unshift({
          id: Date.now(),
          type: 'approve',
          text: `تبلیغ "${this.selectedAd.title}" تایید شد`,
          time: 'همین حالا',
          user: this.supervisor.name
        });
        
        this.$bvToast.toast('تبلیغ تایید شد', {
          title: 'تایید موفق',
          variant: 'success',
          solid: true
        });
        
        this.calculateStats();
        this.showAdModal = false;
      }
    },
    
    rejectSelectedAd() {
      if (this.selectedAd && this.adReviewComment) {
        this.selectedAd.status = 'rejected';
        
        if (!this.selectedAd.reviews) {
          this.selectedAd.reviews = [];
        }
        
        this.selectedAd.reviews.push({
          id: Date.now(),
          reviewer: this.supervisor.name,
          date: this.getCurrentDate(),
          status: 'rejected',
          comment: this.adReviewComment
        });
        
        // Add to activities
        this.recentActivities.unshift({
          id: Date.now(),
          type: 'reject',
          text: `تبلیغ "${this.selectedAd.title}" رد شد`,
          time: 'همین حالا',
          user: this.supervisor.name
        });
        
        this.$bvToast.toast('تبلیغ رد شد', {
          title: 'رد تبلیغ',
          variant: 'warning',
          solid: true
        });
        
        this.calculateStats();
        this.showAdModal = false;
      }
    },
    
    // Bulk Actions
    bulkApprove() {
      this.$bvModal.msgBoxConfirm(`آیا می‌خواهید ${this.selectedDocuments.length} مدرک انتخاب شده را تایید کنید؟`, {
        title: 'تایید گروهی',
        size: 'md',
        buttonSize: 'sm',
        okVariant: 'success',
        okTitle: 'تایید همه',
        cancelTitle: 'لغو',
        centered: true
      }).then(value => {
        if (value) {
          this.selectedDocuments.forEach(doc => {
            doc.status = 'approved';
            doc.reviewedBy = this.supervisor.name;
            doc.reviewedAt = this.getCurrentDate();
          });
          
          this.$bvToast.toast(`${this.selectedDocuments.length} مدرک تایید شد`, {
            title: 'تایید گروهی موفق',
            variant: 'success',
            solid: true
          });
          
          this.selectedDocuments = [];
          this.calculateStats();
        }
      });
    },
    
    bulkReject() {
      this.$bvModal.msgBoxConfirm(`آیا می‌خواهید ${this.selectedDocuments.length} مدرک انتخاب شده را رد کنید؟`, {
        title: 'رد گروهی',
        size: 'md',
        buttonSize: 'sm',
        okVariant: 'danger',
        okTitle: 'رد همه',
        cancelTitle: 'لغو',
        centered: true
      }).then(value => {
        if (value) {
          this.selectedDocuments.forEach(doc => {
            doc.status = 'rejected';
            doc.reviewedBy = this.supervisor.name;
            doc.reviewedAt = this.getCurrentDate();
            doc.rejectionReason = 'رد گروهی';
          });
          
          this.$bvToast.toast(`${this.selectedDocuments.length} مدرک رد شد`, {
            title: 'رد گروهی',
            variant: 'warning',
            solid: true
          });
          
          this.selectedDocuments = [];
          this.calculateStats();
        }
      });
    },
    
    clearSelection() {
      this.selectedDocuments = [];
    },
    
    // Statistics
    calculateStats() {
      this.stats.totalCandidates = this.candidates.length;
      this.stats.pendingDocuments = this.candidates.filter(c => c.status === 'pending').length;
      this.stats.approvedCandidates = this.candidates.filter(c => c.status === 'approved').length;
      this.stats.rejectedCandidates = this.candidates.filter(c => c.status === 'rejected').length;
      
      this.stats.pendingAds = this.advertisements.filter(a => a.status === 'pending').length;
      this.stats.approvedAds = this.advertisements.filter(a => a.status === 'approved').length;
      this.stats.rejectedAds = this.advertisements.filter(a => a.status === 'rejected').length;
      
      this.updateCharts();
    },
    
    // Charts
    initializeCharts() {
      this.createDocumentsChart();
      this.createAdsChart();
    },
    
    createDocumentsChart() {
      const ctx = this.$refs.documentsChart?.getContext('2d');
      if (!ctx) return;
      
      if (this.documentsChart) {
        this.documentsChart.destroy();
      }
      
      this.documentsChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['تایید شده', 'در انتظار', 'رد شده'],
          datasets: [{
            data: [
              this.stats.approvedCandidates,
              this.stats.pendingDocuments,
              this.stats.rejectedCandidates
            ],
            backgroundColor: [
              '#4CAF50',
              '#FFC107',
              '#F44336'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              rtl: true
            }
          }
        }
      });
    },
    
    createAdsChart() {
      const ctx = this.$refs.adsChart?.getContext('2d');
      if (!ctx) return;
      
      if (this.adsChart) {
        this.adsChart.destroy();
      }
      
      this.adsChart = new Chart(ctx, {
        type: 'bar',
        data: {
          labels: ['تایید شده', 'در انتظار', 'رد شده'],
          datasets: [{
            label: 'تعداد تبلیغات',
            data: [
              this.stats.approvedAds,
              this.stats.pendingAds,
              this.stats.rejectedAds
            ],
            backgroundColor: [
              '#4CAF50',
              '#FFC107',
              '#F44336'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          maintainAspectRatio: false,
          scales: {
            y: {
              beginAtZero: true,
              ticks: {
                stepSize: 1
              }
            }
          }
        }
      });
    },
    
    updateCharts() {
      if (this.documentsChart) {
        this.documentsChart.data.datasets[0].data = [
          this.stats.approvedCandidates,
          this.stats.pendingDocuments,
          this.stats.rejectedCandidates
        ];
        this.documentsChart.update();
      }
      
      if (this.adsChart) {
        this.adsChart.data.datasets[0].data = [
          this.stats.approvedAds,
          this.stats.pendingAds,
          this.stats.rejectedAds
        ];
        this.adsChart.update();
      }
    },
    
    // Utility
    getCurrentDate() {
      return new Date().toLocaleDateString('fa-IR');
    },
    
    filterDocuments() {
      // Just trigger computed property
    },
    
    filterAds() {
      // Just trigger computed property
    }
  }
};
</script>

<style scoped>
.supervisor-dashboard {
  background: linear-gradient(135deg, #f5f7fa 0%, #e4e8f0 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Header */
.supervisor-header {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.supervisor-icon {
  font-size: 2.5rem;
  color: #4CAF50;
}

.supervisor-info {
  background: rgba(255, 255, 255, 0.1);
  padding: 15px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.supervisor-name {
  font-size: 1.2rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.supervisor-role {
  font-size: 0.9rem;
  opacity: 0.9;
  margin-bottom: 8px;
}

/* Candidate Card */
.candidate-card {
  border-radius: 12px;
  border: 1px solid #e0e0e0;
  transition: all 0.3s ease;
}

.candidate-card:hover {
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
  border-color: #2196F3;
}

.candidate-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #f0f0f0;
}

.candidate-status {
  display: flex;
  align-items: center;
  margin-top: 10px;
}

/* Documents List */
.documents-list {
  border-top: 1px solid #f0f0f0;
  border-bottom: 1px solid #f0f0f0;
  padding: 15px 0;
  margin: 15px 0;
}

.document-item {
  padding: 10px;
  border-bottom: 1px solid #f5f5f5;
}

.document-item:last-child {
  border-bottom: none;
}

.document-info {
  display: flex;
  align-items: center;
  flex: 1;
}

.document-actions {
  display: flex;
  gap: 5px;
}

.document-status.status-approved {
  color: #4CAF50;
  background: rgba(76, 175, 80, 0.1);
  padding: 5px 10px;
  border-radius: 4px;
  border-right: 3px solid #4CAF50;
}

.document-status.status-rejected {
  color: #F44336;
  background: rgba(244, 67, 54, 0.1);
  padding: 5px 10px;
  border-radius: 4px;
  border-right: 3px solid #F44336;
}

/* Candidate Actions */
.candidate-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  flex-wrap: wrap;
}

/* Ads Table */
.ad-thumbnail {
  width: 60px;
  height: 40px;
  object-fit: cover;
  border-radius: 4px;
  cursor: pointer;
  border: 1px solid #e0e0e0;
}

.ad-thumbnail.placeholder {
  background: #f5f5f5;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999;
}

/* Statistics Cards */
.stat-card {
  border: none;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.08);
  border-radius: 10px;
}

.stat-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 15px;
  font-size: 1.5rem;
}

.stat-icon.total {
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
  color: white;
}

.stat-icon.pending {
  background: linear-gradient(135deg, #FF9800 0%, #FFC107 100%);
  color: white;
}

.stat-icon.approved {
  background: linear-gradient(135deg, #4CAF50 0%, #8BC34A 100%);
  color: white;
}

.stat-icon.rejected {
  background: linear-gradient(135deg, #F44336 0%, #EF5350 100%);
  color: white;
}

.stat-number {
  font-size: 2rem;
  font-weight: bold;
  color: #2c3e50;
  margin-bottom: 5px;
}

.stat-label {
  color: #666;
  font-size: 0.9rem;
}

/* Charts */
.chart-container {
  height: 250px;
  position: relative;
}

/* Activities List */
.activities-list {
  max-height: 300px;
  overflow-y: auto;
}

.activity-item {
  display: flex;
  align-items: center;
  padding: 12px;
  border-bottom: 1px solid #f0f0f0;
}

.activity-item:last-child {
  border-bottom: none;
}

.activity-icon {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f5f5;
  color: #666;
  margin-left: 15px;
}

.activity-content {
  flex: 1;
}

.activity-text {
  font-weight: 500;
  margin-bottom: 3px;
}

.activity-time {
  font-size: 0.85rem;
  color: #666;
}

/* Modals */
.document-viewer {
  padding: 10px;
}

.image-viewer img {
  max-height: 400px;
  object-fit: contain;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.pdf-frame {
  width: 100%;
  height: 400px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
}

.document-details {
  background: #f8f9fa;
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.detail-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #e0e0e0;
}

.detail-item:last-child {
  border-bottom: none;
}

/* Candidate Details */
.candidate-photo {
  width: 150px;
  height: 150px;
  border-radius: 50%;
  object-fit: cover;
  border: 5px solid #f0f0f0;
}

.candidate-meta {
  display: flex;
  gap: 20px;
  flex-wrap: wrap;
  margin-top: 15px;
}

.meta-item {
  display: flex;
  align-items: center;
  color: #666;
}

.documents-summary {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.summary-item {
  text-align: center;
  padding: 15px;
  border-radius: 8px;
  background: white;
}

.summary-item.total {
  border: 2px solid #2196F3;
}

.summary-item.approved {
  border: 2px solid #4CAF50;
}

.summary-item.pending {
  border: 2px solid #FFC107;
}

.summary-number {
  font-size: 1.8rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.summary-label {
  color: #666;
  font-size: 0.9rem;
}

.bio-text {
  line-height: 1.8;
  color: #555;
  text-align: justify;
}

.program-list {
  padding-right: 20px;
  color: #555;
}

.program-list li {
  margin-bottom: 8px;
  line-height: 1.6;
}

/* Ad Details */
.ad-description {
  line-height: 1.8;
  color: #555;
  text-align: justify;
}

.info-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
}

.info-item:last-child {
  border-bottom: none;
}

.review-item {
  padding: 10px;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 10px;
}

.review-item:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 5px;
}

.review-comment {
  color: #555;
  line-height: 1.6;
  padding: 10px;
  background: #f8f9fa;
  border-radius: 4px;
  border-right: 3px solid #2196F3;
}

/* Bulk Actions */
.bulk-actions {
  position: fixed;
  bottom: 20px;
  left: 20px;
  right: 20px;
  z-index: 1000;
}

.bulk-card {
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.1);
  border-radius: 10px;
  background: white;
}

/* Responsive Design */
@media (max-width: 768px) {
  .supervisor-info {
    margin-top: 15px;
    text-align: center;
  }
  
  .candidate-header {
    flex-direction: column;
    text-align: center;
  }
  
  .candidate-avatar {
    margin: 0 auto 15px;
  }
  
  .candidate-status {
    justify-content: center;
  }
  
  .candidate-actions {
    justify-content: center;
  }
  
  .stat-card {
    margin-bottom: 15px;
  }
  
  .bulk-actions {
    left: 10px;
    right: 10px;
    bottom: 10px;
  }
}
</style>