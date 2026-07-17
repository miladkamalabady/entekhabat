<template>
  <div class="objection-page-modern">
    <!-- فاصله از topbar -->
    <div class="page-spacer"></div>

    <!-- Header با گرادیانت آبی (هماهنگ با سایر صفحات) -->
    <div class="objection-header-modern">
      <div class="container-fluid">
        <div class="header-content">
          <div class="header-title">
            <div class="icon-wrapper">
              <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
                <path
                  d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                  stroke-linecap="round" />
                <path d="M18 6L6 18" stroke-width="1.5" stroke-linecap="round" />
              </svg>
            </div>
            <div>
              <h2>اعتراض به تصمیم هیأت نظارت</h2>
              <p>فرم ثبت اعتراض به تصمیمات هیأت نظارت انتخابات</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="container-fluid objection-container">

      <!-- Tabs with Modern Design -->
      <div class="tabs-modern">
        <div class="tab-header">
          <button class="tab-btn" :class="{ active: activeTab === 0 }" @click="activeTab = 0">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path
                d="M12 8V12M12 16H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                stroke-width="1.5" />
            </svg>
            <span>ثبت اعتراض جدید</span>
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 1 }" @click="activeTab = 1; loadUserComplaints()">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M20 21V19C20 16.8 18.2 15 16 15H8C5.8 15 4 16.8 4 19V21" stroke-width="1.5" />
              <circle cx="12" cy="7" r="4" stroke-width="1.5" />
            </svg>
            <span>اعتراضات من</span>
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 2 }" @click="activeTab = 2">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <circle cx="11" cy="11" r="8" stroke-width="1.5" />
              <path d="M21 21L17 17" stroke-width="1.5" stroke-linecap="round" />
            </svg>
            <span>پیگیری اعتراض</span>
          </button>
          <button class="tab-btn" :class="{ active: activeTab === 3 }" @click="activeTab = 3">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path d="M12 2L2 7L12 12L22 7L12 2Z" stroke-width="1.5" />
              <path d="M2 17L12 22L22 17" stroke-width="1.5" />
              <path d="M2 12L12 17L22 12" stroke-width="1.5" />
            </svg>
            <span>قوانین و راهنما</span>
          </button>
        </div>

        <!-- Tab Content -->
        <div class="tab-content-modern">

          <!-- Tab 1: ثبت اعتراض جدید -->
          <div v-if="activeTab === 0" class="tab-pane">
            <div class="form-card-modern">
              <!-- هشدار محدودیت تعداد اعتراض -->
              <div v-if="complaintCount >= 2" class="alert-warning-modern">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d="M12 9V13M12 17H12.01M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z" stroke-width="1.5" />
                </svg>
                <span>شما قبلاً حداکثر تعداد مجاز اعتراض (۲ بار) را ثبت کرده‌اید.</span>
              </div>

              <b-form @submit.prevent="validateBeforeSubmit">
                <!-- اطلاعات کاندیدا (در صورت نیاز) -->
                <div v-if="showCandidateFields" class="candidate-fields">
                  <h6 class="section-title-mini">اطلاعات کاندیدا</h6>
                  <div class="row-modern">
                    <div class="col-modern">
                      <div class="form-group-modern">
                        <label>نام کاندیدا</label>
                        <input type="text" v-model="complaintData.candidateName" class="input-modern"
                          placeholder="نام کامل کاندیدا">
                      </div>
                    </div>
                    <div class="col-modern">
                      <div class="form-group-modern">
                        <label>منطقه انتخابی</label>
                        <input type="text" v-model="complaintData.candidateRegion" class="input-modern"
                          placeholder="منطقه انتخابی">
                      </div>
                    </div>
                  </div>
                </div>

                <!-- شرح اعتراض -->
                <div class="form-group-modern">
                  <label>شرح کامل اعتراض <span class="required">*</span></label>
                  <textarea v-model="complaintData.description" rows="6" class="textarea-modern"
                    :class="{ 'is-invalid': formState.description === false }"
                    placeholder="شرح کامل اعتراض خود را با ذکر دلایل و مستندات وارد کنید (حداقل ۵۰ کاراکتر)"></textarea>
                  <div class="char-counter">{{ complaintData.description.length }} / 2000 کاراکتر</div>
                  <div v-if="formState.description === false" class="invalid-feedback">لطفا شرح اعتراض را حداقل ۵۰
                    کاراکتر وارد کنید</div>
                </div>

                <!-- بارگذاری مستندات - نسخه اصلاح شده با محدودیت‌ها -->
                <div class="form-group-modern">
                  <label>بارگذاری مستندات و مدارک اثباتی <span class="required">*</span></label>
                  
                  <!-- نمایش محدودیت‌ها -->
                  <div class="file-limits-info">
                    <span class="limit-badge">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z" stroke-width="1.5" />
                        <path d="M12 8V12M12 16H12.01" stroke-width="1.5" stroke-linecap="round" />
                      </svg>
                      حداکثر ۳ فایل
                    </span>
                    <span class="limit-badge">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z" stroke-width="1.5" />
                      </svg>
                      حداکثر ۱ مگابایت هر فایل
                    </span>
                    <span class="limit-badge">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z" stroke-width="1.5" />
                      </svg>
                      PDF, JPG, PNG, DOC, DOCX
                    </span>
                  </div>

                  <div class="file-upload-modern" :class="{ 'is-invalid': formState.documents === false }">
                    <input type="file" ref="fileInput" multiple @change="onDocumentsChange" class="file-input-hidden"
                      accept=".pdf,.jpg,.jpeg,.png,.doc,.docx">
                    <div class="file-upload-area" @click="$refs.fileInput.click()">
                      <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M12 4V20M20 12H4" stroke-width="1.5" stroke-linecap="round" />
                      </svg>
                      <span>فایل‌های خود را انتخاب کنید</span>
                      <small>فرمت‌های مجاز: PDF, JPG, PNG, DOC, DOCX | حداکثر حجم: ۱ مگابایت | حداکثر تعداد: ۳ فایل</small>
                    </div>

                    <!-- نمایش تعداد فایل‌های انتخاب شده -->
                    <div v-if="uploadedFiles.length > 0" class="files-counter">
                      <span>{{ uploadedFiles.length }} از ۳ فایل انتخاب شده</span>
                    </div>

                    <!-- لیست فایل‌های انتخاب شده با پیش‌نمایش و وضعیت حجم -->
                    <div v-if="uploadedFiles.length > 0" class="files-list-modern">
                      <div v-for="(file, index) in uploadedFiles" :key="index" class="file-item-modern"
                        :class="{ 'file-error': file.size > maxFileSize }">
                        <div class="file-info">
                          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <path
                              d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z"
                              stroke-width="1.5" />
                          </svg>
                          <span class="file-name">{{ file.name }}</span>
                          <small class="file-size">{{ formatFileSize(file.size) }}</small>
                          <span v-if="file.size > maxFileSize" class="file-error-badge">
                            حجم بیش از حد مجاز
                          </span>
                        </div>
                        <button type="button" class="remove-file" @click.stop="removeFile(index)">
                          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                            <path d="M18 6L6 18M6 6L18 18" stroke-width="1.5" stroke-linecap="round" />
                          </svg>
                        </button>
                      </div>
                    </div>

                    <!-- نمایش خطاهای اعتبارسنجی فایل -->
                    <div v-if="fileValidationErrors.length > 0" class="file-errors">
                      <div v-for="(error, index) in fileValidationErrors" :key="index" class="file-error-item">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#ef4444">
                          <circle cx="12" cy="12" r="10" stroke-width="1.5" />
                          <path d="M12 8V12M12 16H12.01" stroke-width="1.5" stroke-linecap="round" />
                        </svg>
                        {{ error }}
                      </div>
                    </div>
                  </div>
                  <div v-if="formState.documents === false" class="invalid-feedback">لطفا حداقل یک مدرک بارگذاری کنید
                  </div>
                </div>

                <!-- دکمه‌های اقدام -->
                <div class="form-actions">
                  <button type="button" class="btn-outline-modern" @click="resetForm">
                    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                      <path d="M1 12C1 12 4 4 12 4C20 4 23 12 23 12C23 12 20 20 12 20C4 20 1 12 1 12Z"
                        stroke-width="1.5" />
                    </svg>
                    پاک کردن فرم
                  </button>
                  <button type="submit" class="btn-primary-modern" :disabled="submitting || complaintCount >= 2">
                    <span v-if="submitting" class="spinner-small"></span>
                    <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                      <path d="M20 6L9 17L4 12" stroke-width="2" stroke-linecap="round" />
                    </svg>
                    {{ submitting ? 'در حال ارسال...' : 'ثبت اعتراض' }}
                  </button>
                </div>
              </b-form>
            </div>
          </div>

          <!-- Tab 2: اعتراضات من -->
          <div v-if="activeTab === 1" class="tab-pane">
            <div v-if="userComplaints.length > 0" class="complaints-list-modern">
              <div v-for="complaint in userComplaints" :key="complaint.id" class="complaint-card-modern">
                <div class="complaint-card-header">
                  <div class="complaint-info">
                    <h5>{{ complaint.subject || 'اعتراض' }}</h5>
                    <div class="complaint-meta">
                      <span class="badge-status" :class="`status-${complaint.status}`">
                        {{ getComplaintStatusText(complaint.status) }}
                      </span>
                      <span class="meta-date">
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path
                            d="M3 6H21M8 3V6M16 3V6M4 10H20M5 21H19C20.1 21 21 20.1 21 19V7C21 5.9 20.1 5 19 5H5C3.9 5 3 5.9 3 7V19C3 20.1 3.9 21 5 21Z"
                            stroke-width="1.5" />
                        </svg>
                        {{ complaint.submittedDate }}
                      </span>
                      <span class="meta-tracking">
                        کد پیگیری: {{ complaint.trackingCode }}
                      </span>
                    </div>
                  </div>
                  <div class="complaint-actions">
                    <button class="action-icon view" @click="viewComplaintDetails(complaint)" title="مشاهده">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M1 12C1 12 5 4 12 4C19 4 23 12 23 12C23 12 19 20 12 20C5 20 1 12 1 12Z"
                          stroke-width="1.5" />
                        <circle cx="12" cy="12" r="3" stroke-width="1.5" />
                      </svg>
                    </button>
                    <button v-if="complaint.status === 'pending'" class="action-icon edit"
                      @click="editComplaint(complaint)" title="ویرایش">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M17 3L21 7L7 21H3V17L17 3Z" stroke-width="1.5" />
                      </svg>
                    </button>
                    <button v-if="complaint.status === 'pending'" class="action-icon delete"
                      @click="cancelComplaint(complaint)" title="لغو">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M18 6L6 18M6 6L18 18" stroke-width="1.5" stroke-linecap="round" />
                      </svg>
                    </button>
                  </div>
                </div>
                <div class="complaint-card-body">
                  <p class="complaint-preview">{{ complaint.description?.substring(0, 150) }}...</p>
                  <div class="complaint-footer">
                    <span class="doc-count">
                      <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path
                          d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z"
                          stroke-width="1.5" />
                      </svg>
                      {{ complaint.documentsCount || 0 }} سند ضمیمه
                    </span>
                    <span class="last-update">آخرین به‌روزرسانی: {{ complaint.lastUpdate || complaint.submittedDate
                    }}</span>
                  </div>
                </div>
              </div>
            </div>
            <div v-else class="empty-state-modern">
              <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#cbd5e1">
                <path d="M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                  stroke-width="1.5" />
                <path d="M8 12H16M12 8V16" stroke-width="1.5" stroke-linecap="round" />
              </svg>
              <h5>هیچ اعتراضی ثبت نکرده‌اید</h5>
              <p>برای ثبت اعتراض جدید، به تب "ثبت اعتراض جدید" مراجعه کنید.</p>
            </div>
          </div>

          <!-- Tab 3: پیگیری اعتراض -->
          <div v-if="activeTab === 2" class="tab-pane">
            <div class="tracking-card-modern">
              <div class="tracking-form">
                <div class="form-group-modern">
                  <label>شماره پیگیری (کد رهگیری)</label>
                  <div class="tracking-input-group">
                    <input type="text" v-model="trackingCode" class="input-modern"
                      placeholder="کد ۱۲ رقمی پیگیری خود را وارد کنید">
                    <button class="btn-search" @click="trackComplaint" :disabled="trackingLoading">
                      <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <circle cx="11" cy="11" r="8" stroke-width="1.5" />
                        <path d="M21 21L17 17" stroke-width="1.5" stroke-linecap="round" />
                      </svg>
                      {{ trackingLoading ? 'در حال جستجو...' : 'پیگیری' }}
                    </button>
                  </div>
                </div>
              </div>

              <!-- نتیجه پیگیری -->
              <div v-if="trackingResult" class="tracking-result">
                <div class="result-header">
                  <h5>وضعیت اعتراض</h5>
                  <span class="badge-status" :class="`status-${trackingResult.status}`">
                    {{ getComplaintStatusText(trackingResult.status) }}
                  </span>
                </div>

                <div class="timeline-modern">
                  <div v-for="(step, index) in trackingResult.timeline" :key="index" class="timeline-step-modern"
                    :class="{ completed: step.completed, active: step.active }">
                    <div class="timeline-icon">
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path v-if="step.icon === 'clipboard-check'"
                          d="M9 12L11 14L15 10M12 2C6.48 2 2 6.48 2 12C2 17.52 6.48 22 12 22C17.52 22 22 17.52 22 12C22 6.48 17.52 2 12 2Z"
                          stroke-width="1.5" />
                        <path v-else-if="step.icon === 'person-check'"
                          d="M20 21V19C20 16.8 18.2 15 16 15H8C5.8 15 4 16.8 4 19V21" stroke-width="1.5" />
                        <circle v-else cx="12" cy="7" r="4" stroke-width="1.5" />
                      </svg>
                    </div>
                    <div class="timeline-content">
                      <div class="timeline-title">{{ step.title }}</div>
                      <div class="timeline-date">{{ step.date }}</div>
                      <div class="timeline-desc">{{ step.description }}</div>
                    </div>
                  </div>
                </div>

                <div class="status-details">
                  <div class="detail-row">
                    <span>موضوع:</span>
                    <strong>{{ trackingResult.subject || '-' }}</strong>
                  </div>
                  <div class="detail-row">
                    <span>تاریخ ثبت:</span>
                    <strong>{{ trackingResult.submittedDate }}</strong>
                  </div>
                  <div class="detail-row">
                    <span>آخرین به‌روزرسانی:</span>
                    <strong>{{ trackingResult.lastUpdate }}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Tab 4: قوانین و راهنما -->
          <div v-if="activeTab === 3" class="tab-pane">
            <div class="rules-card-modern">
              <div class="rules-section">
                <h6>📖 مقدمه</h6>
                <p>سیستم اعتراضات هیأت نظارت انتخابات، بستری شفاف و قانونی برای رسیدگی به اعتراضات مربوط به تصمیمات هیأت
                  نظارت فراهم می‌کند.</p>
              </div>
              <div class="rules-section">
                <h6>✅ شرایط و ضوابط</h6>
                <ul>
                  <li>اعتراض باید حداکثر تا ۷ روز پس از اعلام تصمیم هیأت نظارت ثبت شود.</li>
                  <li>اعتراض باید مستند و همراه با دلایل محکمه‌پسند ارائه شود.</li>
                  <li>اعتراضات توهین‌آمیز یا فاقد مستندات معتبر بررسی نخواهند شد.</li>
                  <li>هر داوطلب حداکثر <strong>۲ بار</strong> مجاز به ثبت اعتراض می‌باشد.</li>
                  <li>حداکثر <strong>۳ فایل</strong> با حجم <strong>۱ مگابایت</strong> هر فایل مجاز است.</li>
                </ul>
              </div>
              <div class="rules-section">
                <h6>⏱️ مهلت‌های قانونی</h6>
                <div class="time-grid">
                  <div class="time-item"><span>ثبت اعتراض:</span><strong>۷ روز کاری</strong></div>
                  <div class="time-item"><span>بررسی اولیه:</span><strong>۳ روز کاری</strong></div>
                  <div class="time-item"><span>بررسی تخصصی:</span><strong>۱۴ روز کاری</strong></div>
                  <div class="time-item"><span>صدور رأی:</span><strong>۷ روز کاری</strong></div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- مودال جزئیات اعتراض - با قابلیت دانلود فایل‌ها -->
    <b-modal v-model="showComplaintModal" title="جزئیات اعتراض" size="lg" hide-footer centered
      class="modal-glass-modern">
      <div v-if="selectedComplaint" class="modal-body-custom">
        <div class="modal-header-info">
          <h5>{{ selectedComplaint.subject || 'اعتراض' }}</h5>
          <span class="badge-status" :class="`status-${selectedComplaint.status}`">
            {{ getComplaintStatusText(selectedComplaint.status) }}
          </span>
        </div>

        <div class="modal-tracking-code">
          <span>کد پیگیری:</span>
          <strong>{{ selectedComplaint.trackingCode }}</strong>
        </div>

        <div class="modal-section">
          <label>شرح اعتراض</label>
          <p>{{ selectedComplaint.description }}</p>
        </div>

        <!-- نمایش فایل‌های ضمیمه شده -->
        <div v-if="selectedComplaint.documents && selectedComplaint.documents.length > 0" class="modal-section">
          <label>مدارک ضمیمه ({{ selectedComplaint.documents.length }} فایل)</label>
          <div class="documents-list-modal">
            <div v-for="(doc, index) in selectedComplaint.documents" :key="index" class="document-item-modal">
              <div class="document-info">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                  <path d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z"
                    stroke-width="1.5" />
                  <path d="M8 8H16M8 12H12" stroke-width="1.5" stroke-linecap="round" />
                </svg>
                <span class="doc-name">{{ doc.name || doc.file_name }}</span>
                <small class="doc-size" v-if="doc.size">{{ formatFileSize(doc.size) }}</small>
              </div>
              <div class="document-actions">
                <button class="btn-download" @click="downloadDocument(doc)" title="دانلود">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                    <path d="M12 3V16M12 16L9 13M12 16L15 13" stroke-width="1.5" stroke-linecap="round" />
                    <path d="M5 21H19" stroke-width="1.5" stroke-linecap="round" />
                  </svg>
                  <span>دانلود</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- پیام در صورت نبود فایل -->
        <div v-else class="modal-section">
          <label>مدارک ضمیمه</label>
          <div class="no-documents">
            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="#cbd5e1">
              <path d="M4 4H20C21.1 4 22 4.9 22 6V18C22 19.1 21.1 20 20 20H4C2.9 20 2 19.1 2 18V6C2 4.9 2.9 4 4 4Z"
                stroke-width="1.5" />
            </svg>
            <span>هیچ مدرکی ضمیمه نشده است</span>
          </div>
        </div>

        <div class="modal-footer-actions">
          <button class="btn-outline-modern" @click="showComplaintModal = false">بستن</button>
        </div>
      </div>
    </b-modal>

    <!-- مودال موفقیت -->
    <b-modal v-model="showSuccessModal" title="ثبت اعتراض با موفقیت انجام شد" hide-footer centered
      class="modal-glass-modern success-modal-glass">
      <div class="success-modal-content">
        <div class="success-icon">
          <svg width="64" height="64" viewBox="0 0 24 24" fill="none">
            <circle cx="12" cy="12" r="10" fill="#10b981" />
            <path d="M8 12L11 15L16 9" stroke="white" stroke-width="2" stroke-linecap="round" />
          </svg>
        </div>
        <h5>اعتراض شما با موفقیت ثبت شد</h5>
        <p>کد پیگیری زیر را برای پیگیری حفظ کنید.</p>
        <div class="tracking-code-box">{{ newTrackingCode }}</div>
        <button class="btn-primary-modern" @click="showSuccessModal = false">متوجه شدم</button>
      </div>
    </b-modal>
  </div>
</template>

<script>
import { mapActions, mapGetters } from 'vuex'
import axios from 'axios'

const createDefaultComplaintData = () => ({
  decisionType: 'province',
  caseNumber: '',
  candidateName: '',
  candidateRegion: '',
  candidatePosition: '',
  subject: '-',
  description: '',
  reasons: [],
  documents: [],
  urgency: 'normal'
})
import { apiUrlrtb } from '../../constants/config'
export default {
  name: 'UserComplaint',
  data() {
    return {
      apiUrlrtb,
      activeTab: 0,
      submitting: false,
      trackingLoading: false,
      complaintData: createDefaultComplaintData(),
      formState: {
        description: null,
        documents: null
      },
      uploadedFiles: [],
      userComplaints: [],
      trackingCode: '',
      trackingResult: null,
      showComplaintModal: false,
      showSuccessModal: false,
      selectedComplaint: null,
      newTrackingCode: '',
      complaintCount: 0,
      // محدودیت‌های فایل
      maxFileCount: 3,
      maxFileSize: 1 * 1024 * 1024, // 1 مگابایت
      allowedFileTypes: ['pdf', 'jpg', 'jpeg', 'png', 'doc', 'docx'],
      fileValidationErrors: []
    }
  },
  computed: {
    ...mapGetters(['currentUser']),
    showCandidateFields() {
      return false
    }
  },
  async mounted() {
    await this.loadUserComplaints()
  },
  methods: {
    ...mapActions(['getObjections', 'saveObjection', 'updateObjectionStatus','downloadObjectionFile']),

    async downloadDocument(doc) {
      if (!doc || !doc.id) {
        this.$bvToast.toast('اطلاعات فایل موجود نیست', { title: 'خطا', variant: 'danger', solid: true })
        return
      }

      try {
        const token = this.currentUser.token
        const response = await this.downloadObjectionFile({id:`${doc.id}`})
        const blob = new Blob([response])
        const url = window.URL.createObjectURL(blob)
        const link = document.createElement('a')
        link.href = url
        link.download = doc.name || doc.file_name || 'file'
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        window.URL.revokeObjectURL(url)

        this.$bvToast.toast('دانلود فایل شروع شد', { title: 'موفق', variant: 'success', solid: true })

      } catch (error) {
        console.error('Download error:', error)
        this.$bvToast.toast('خطا در دانلود فایل', { title: 'خطا', variant: 'danger', solid: true })
      }
    },


    async loadUserComplaints() {
      try {
        const response = await this.getObjections({})
        this.userComplaints = (response?.data || []).map(complaint => ({
          ...complaint,
          documents: complaint.documents || [],
          documentsCount: complaint.documents?.length || complaint.documentsCount || 0
        }))
        this.complaintCount = this.userComplaints.filter(c => c.status !== 'cancelled').length
      } catch (error) {
        console.error('Error loading complaints:', error)
        this.userComplaints = []
        this.complaintCount = 0
      }
    },

    viewComplaintDetails(complaint) {
      this.selectedComplaint = {
        ...complaint,
        documents: complaint.documents || []
      }
      this.showComplaintModal = true
    },

    getDefaultComplaintData() {
      return createDefaultComplaintData()
    },

    // اعتبارسنجی فایل‌ها
    validateFiles(files) {
      this.fileValidationErrors = []
      const validFiles = []

      for (const file of files) {
        // بررسی تعداد
        if (validFiles.length >= this.maxFileCount) {
          this.fileValidationErrors.push(`حداکثر ${this.maxFileCount} فایل مجاز است`)
          break
        }

        // بررسی حجم
        if (file.size > this.maxFileSize) {
          this.fileValidationErrors.push(`فایل ${file.name} حجم بیشتر از ۱ مگابایت دارد`)
          continue
        }

        // بررسی نوع فایل
        const extension = file.name.split('.').pop().toLowerCase()
        if (!this.allowedFileTypes.includes(extension)) {
          this.fileValidationErrors.push(`نوع فایل ${file.name} مجاز نیست (فرمت‌های مجاز: ${this.allowedFileTypes.join(', ')})`)
          continue
        }

        validFiles.push(file)
      }

      return validFiles
    },

    onDocumentsChange(event) {
      const files = Array.from(event.target.files)
      
      // اعتبارسنجی فایل‌ها
      const validFiles = this.validateFiles(files)
      
      // اضافه کردن فایل‌های معتبر
      const newFiles = [...this.uploadedFiles, ...validFiles]
      
      // محدودیت تعداد کل
      if (newFiles.length > this.maxFileCount) {
        this.fileValidationErrors.push(`حداکثر ${this.maxFileCount} فایل مجاز است`)
        this.uploadedFiles = newFiles.slice(0, this.maxFileCount)
      } else {
        this.uploadedFiles = newFiles
      }

      // به‌روزرسانی وضعیت فرم
      this.formState.documents = this.uploadedFiles.length > 0 ? true : null

      // نمایش پیام خطا
      if (this.fileValidationErrors.length > 0) {
        this.$bvToast.toast(this.fileValidationErrors.join('\n'), {
          title: 'خطا در انتخاب فایل',
          variant: 'danger',
          solid: true
        })
      }

      // ریست کردن input فایل
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = ''
      }
    },

    removeFile(index) {
      this.uploadedFiles.splice(index, 1)
      this.formState.documents = this.uploadedFiles.length > 0 ? true : null
      this.fileValidationErrors = []
    },

    formatFileSize(bytes) {
      if (bytes === 0) return '0 بایت'
      const k = 1024
      const sizes = ['بایت', 'کیلوبایت', 'مگابایت']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
    },

    // بررسی اینکه آیا فایل‌ها معتبر هستند
    areFilesValid() {
      if (this.uploadedFiles.length === 0) return false
      
      // بررسی حجم فایل‌ها
      for (const file of this.uploadedFiles) {
        if (file.size > this.maxFileSize) {
          this.fileValidationErrors.push(`فایل ${file.name} حجم بیشتر از ۱ مگابایت دارد`)
          return false
        }
      }
      
      return true
    },

    validateBeforeSubmit() {
      if (this.complaintCount >= 2) {
        this.$bvToast.toast('شما قبلاً حداکثر تعداد مجاز اعتراض (۲ بار) را ثبت کرده‌اید.', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
        return
      }

      this.fileValidationErrors = []

      // اعتبارسنجی شرح
      if (!this.complaintData.description.trim() || this.complaintData.description.length < 50) {
        this.formState.description = false
      } else {
        this.formState.description = true
      }

      // اعتبارسنجی فایل‌ها
      if (this.uploadedFiles.length === 0) {
        this.formState.documents = false
        this.$bvToast.toast('لطفا حداقل یک مدرک بارگذاری کنید', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
        return
      }

      // اعتبارسنجی حجم فایل‌ها
      for (const file of this.uploadedFiles) {
        if (file.size > this.maxFileSize) {
          this.fileValidationErrors.push(`فایل ${file.name} حجم بیشتر از ۱ مگابایت دارد`)
        }
      }

      if (this.fileValidationErrors.length > 0) {
        this.$bvToast.toast(this.fileValidationErrors.join('\n'), {
          title: 'خطا در فایل‌ها',
          variant: 'danger',
          solid: true
        })
        return
      }

      // اگر همه چیز معتبر است
      if (this.formState.description === true && this.uploadedFiles.length > 0) {
        this.submitComplaint()
      } else {
        this.$bvToast.toast('لطفا تمام فیلدهای الزامی را به درستی تکمیل کنید', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
      }
    },

    async submitComplaint() {
      if (this.complaintCount >= 2) {
        this.$bvToast.toast('شما قبلاً حداکثر تعداد مجاز اعتراض (۲ بار) را ثبت کرده‌اید.', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
        return
      }

      this.submitting = true

      try {
        const formData = new FormData()

        formData.append('decisionType', 'province')
        formData.append('description', this.complaintData.description || '')
        formData.append('subject', '-')
        formData.append('declaration', 'true')

        // فقط فایل‌های معتبر را ارسال کن
        const validFiles = this.uploadedFiles.filter(file => file.size <= this.maxFileSize)
        validFiles.forEach((file, index) => {
              formData.append('documents', file)
        })

        const response = await this.saveObjection(formData)

        if (!response?.status) {
          throw new Error(response?.message || 'خطا در ثبت اعتراض')
        }

        this.newTrackingCode = response?.data?.trackingCode || this.generateTrackingCode()
        this.showSuccessModal = true

        this.resetForm()

        this.activeTab = 1
        await this.loadUserComplaints()

        this.$bvToast.toast('اعتراض شما با موفقیت ثبت شد', {
          title: 'ثبت موفق',
          variant: 'success',
          solid: true
        })

      } catch (error) {
        console.error('Error submitting complaint:', error)
        this.$bvToast.toast(error?.message || 'خطا در ثبت اعتراض. لطفا مجددا تلاش کنید', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
      } finally {
        this.submitting = false
      }
    },

    resetForm() {
      this.complaintData = this.getDefaultComplaintData()
      this.uploadedFiles = []
      this.fileValidationErrors = []
      Object.keys(this.formState).forEach(key => {
        this.formState[key] = null
      })
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = ''
      }
    },

    generateTrackingCode() {
      const date = new Date()
      const year = date.getFullYear() - 621
      const month = (date.getMonth() + 1).toString().padStart(2, '0')
      const day = date.getDate().toString().padStart(2, '0')
      const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
      return `${year}${month}${day}${random}`
    },

    getComplaintStatusVariant(status) {
      const variants = { pending: 'warning', under_review: 'info', approved: 'success', rejected: 'danger', cancelled: 'secondary' }
      return variants[status] || 'secondary'
    },

    getComplaintStatusText(status) {
      const texts = { pending: 'در انتظار', under_review: 'در حال بررسی', approved: 'پذیرفته شده', rejected: 'رد شده', cancelled: 'لغو شده' }
      return texts[status] || status
    },

    editComplaint(complaint) {
      if (this.complaintCount >= 2) {
        this.$bvToast.toast('شما قبلاً حداکثر تعداد مجاز اعتراض را ثبت کرده‌اید.', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
        return
      }
      this.complaintData = { ...this.getDefaultComplaintData(), ...complaint }
      this.uploadedFiles = complaint.documents || []
      this.activeTab = 0
      this.$bvToast.toast('برای ویرایش اعتراض، تغییرات مورد نظر را اعمال کنید', { title: 'ویرایش اعتراض', variant: 'info', solid: true })
    },

    cancelComplaint(complaint) {
      this.$bvModal.msgBoxConfirm('آیا از لغو این اعتراض اطمینان دارید؟', {
        title: 'لغو اعتراض', okVariant: 'danger', okTitle: 'بله، لغو کن', cancelTitle: 'انصراف', centered: true
      }).then(async value => {
        if (value) {
          const response = await this.updateObjectionStatus({ id: complaint.id, status: 'cancelled' })
          if (response?.status) {
            complaint.status = 'cancelled'
            this.$bvToast.toast('اعتراض با موفقیت لغو شد', { title: 'لغو موفق', variant: 'info', solid: true })
            await this.loadUserComplaints()
          }
        }
      })
    },

    async trackComplaint() {
      if (!this.trackingCode || this.trackingCode.trim().length < 8) {
        this.$bvToast.toast('لطفا کد پیگیری معتبر وارد کنید', { title: 'خطا', variant: 'warning' })
        return
      }
      this.trackingLoading = true
      try {
        const response = await this.getObjections({ trackingCode: this.trackingCode })
        if (!response?.status || !response?.data?.length) {
          this.trackingResult = null
          this.$bvToast.toast('کد پیگیری یافت نشد', { title: 'خطا', variant: 'danger' })
          return
        }
        const item = response.data[0]
        this.trackingResult = {
          trackingCode: item.trackingCode,
          subject: item.subject,
          status: item.status,
          submittedDate: item.submittedDate,
          lastUpdate: item.lastUpdate,
          timeline: [
            { title: 'ثبت اعتراض', date: item.submittedDate, description: 'اعتراض توسط کاربر ثبت شد', completed: true, active: false, icon: 'clipboard-check' },
            { title: 'بررسی اولیه', date: item.submittedDate, description: 'در حال بررسی اولیه', completed: false, active: true, icon: 'person-check' },
            { title: 'بررسی تخصصی', date: '-', description: 'در انتظار بررسی تخصصی', completed: false, active: false, icon: 'file-text' },
            { title: 'صدور رأی', date: '-', description: 'در انتظار صدور رأی', completed: false, active: false, icon: 'hammer' }
          ]
        }
      } catch (error) {
        this.$bvToast.toast('خطا در دریافت اطلاعات پیگیری', { title: 'خطا', variant: 'danger', solid: true })
      } finally {
        this.trackingLoading = false
      }
    }
  }
}
</script>

<style scoped>
/* ========== استایل‌های مدرن هماهنگ با سایر صفحات ========== */

.objection-page-modern {
  background: linear-gradient(135deg, #f5f7ff 0%, #eef2fa 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

.page-spacer {
  margin-top: 70px;
}

/* هدر گرادیانتی (هماهنگ با topbar) */
.objection-header-modern {
  background: linear-gradient(135deg, #1e2a6e, #2b3b8a, #1e2a6e);
  padding: 24px 0;
  color: white;
}

.header-content {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 24px;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 16px;
}

.icon-wrapper {
  width: 56px;
  height: 56px;
  background: rgba(255, 255, 255, 0.15);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-title h2 {
  font-size: 1.5rem;
  font-weight: 700;
  margin: 0;
}

.header-title p {
  margin: 0;
  opacity: 0.8;
  font-size: 0.85rem;
}

/* کانتینر اصلی */
.objection-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px;
}

/* تب‌های مدرن */
.tabs-modern {
  background: white;
  border-radius: 24px;
  overflow: hidden;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.06);
}

.tab-header {
  display: flex;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  padding: 0 16px;
}

.tab-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 16px 24px;
  background: none;
  border: none;
  font-size: 0.85rem;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.2s;
  position: relative;
}

.tab-btn svg {
  width: 18px;
  height: 18px;
}

.tab-btn:hover {
  color: #3f51b5;
}

.tab-btn.active {
  color: #3f51b5;
}

.tab-btn.active::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 2px;
  background: #3f51b5;
}

.tab-content-modern {
  padding: 28px;
}

/* فرم مدرن */
.form-card-modern {
  max-width: 800px;
  margin: 0 auto;
}

.form-group-modern {
  margin-bottom: 24px;
}

.form-group-modern label {
  display: block;
  font-size: 0.85rem;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
}

.required {
  color: #ef4444;
  margin-right: 4px;
}

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

.input-modern.is-invalid,
.textarea-modern.is-invalid {
  border-color: #ef4444;
}

.textarea-modern {
  resize: vertical;
}

.char-counter {
  text-align: left;
  font-size: 0.7rem;
  color: #94a3b8;
  margin-top: 6px;
}

.invalid-feedback {
  color: #ef4444;
  font-size: 0.75rem;
  margin-top: 6px;
}

/* اطلاعات محدودیت‌های فایل */
.file-limits-info {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 12px;
}

.limit-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 12px;
  background: #f1f5f9;
  border-radius: 20px;
  font-size: 0.7rem;
  color: #475569;
}

.limit-badge svg {
  flex-shrink: 0;
}

/* آپلود فایل */
.file-upload-modern {
  position: relative;
}

.file-input-hidden {
  display: none;
}

.file-upload-area {
  border: 2px dashed #cbd5e1;
  border-radius: 16px;
  padding: 32px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s;
  background: #f8fafc;
}

.file-upload-area:hover {
  border-color: #3f51b5;
  background: #eef2ff;
}

.file-upload-area svg {
  color: #94a3b8;
  margin-bottom: 12px;
}

.file-upload-area span {
  display: block;
  font-size: 0.85rem;
  color: #475569;
}

.file-upload-area small {
  display: block;
  font-size: 0.7rem;
  color: #94a3b8;
  margin-top: 8px;
}

.files-counter {
  text-align: right;
  font-size: 0.75rem;
  color: #64748b;
  margin-top: 8px;
  padding: 0 4px;
}

.files-list-modern {
  margin-top: 8px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
}

.file-item-modern {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 16px;
  border-bottom: 1px solid #f1f5f9;
}

.file-item-modern:last-child {
  border-bottom: none;
}

.file-item-modern.file-error {
  background: #fef2f2;
  border-color: #fecaca;
}

.file-info {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 1;
  min-width: 0;
}

.file-name {
  font-size: 0.85rem;
  color: #1e293b;
  word-break: break-all;
}

.file-size {
  font-size: 0.7rem;
  color: #94a3b8;
  flex-shrink: 0;
}

.file-error-badge {
  font-size: 0.65rem;
  color: #ef4444;
  background: #fee2e2;
  padding: 2px 8px;
  border-radius: 12px;
  white-space: nowrap;
}

.remove-file {
  background: none;
  border: none;
  cursor: pointer;
  color: #ef4444;
  padding: 4px;
  flex-shrink: 0;
}

/* خطاهای فایل */
.file-errors {
  margin-top: 8px;
}

.file-error-item {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.75rem;
  color: #ef4444;
  padding: 4px 0;
}

/* دکمه‌ها */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 16px;
  margin-top: 32px;
}

.btn-outline-modern {
  padding: 10px 24px;
  border: 1.5px solid #cbd5e1;
  background: white;
  border-radius: 40px;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
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
  display: inline-flex;
  align-items: center;
  gap: 8px;
  transition: all 0.2s;
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

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

/* هشدار محدودیت */
.alert-warning-modern {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 20px;
  background: #fef3c7;
  border-radius: 16px;
  border: 1px solid #f59e0b;
  color: #92400e;
  margin-bottom: 24px;
}

.alert-warning-modern svg {
  flex-shrink: 0;
}

/* کارت‌های اعتراضات */
.complaints-list-modern {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.complaint-card-modern {
  background: #f8fafc;
  border-radius: 20px;
  padding: 20px;
  transition: all 0.2s;
}

.complaint-card-modern:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08);
}

.complaint-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 12px;
}

.complaint-info h5 {
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 8px;
}

.complaint-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
}

.badge-status {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 30px;
  font-size: 0.7rem;
  font-weight: 500;
}

.status-pending {
  background: #fef3c7;
  color: #92400e;
}

.status-under_review {
  background: #dbeafe;
  color: #1e40af;
}

.status-approved {
  background: #dcfce7;
  color: #166534;
}

.status-rejected {
  background: #fee2e2;
  color: #991b1b;
}

.status-cancelled {
  background: #f1f5f9;
  color: #475569;
}

.meta-date,
.meta-tracking {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 0.7rem;
  color: #64748b;
}

.complaint-actions {
  display: flex;
  gap: 8px;
}

.action-icon {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.action-icon.view {
  background: #e0e7ff;
  color: #3f51b5;
}

.action-icon.edit {
  background: #fef3c7;
  color: #d97706;
}

.action-icon.delete {
  background: #fee2e2;
  color: #ef4444;
}

.action-icon:hover {
  transform: scale(1.05);
}

.complaint-preview {
  font-size: 0.85rem;
  color: #475569;
  line-height: 1.5;
  margin-bottom: 12px;
}

.complaint-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.7rem;
  color: #94a3b8;
}

.doc-count {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

/* وضعیت خالی */
.empty-state-modern {
  text-align: center;
  padding: 60px 20px;
}

.empty-state-modern svg {
  margin-bottom: 20px;
}

.empty-state-modern h5 {
  font-size: 1.1rem;
  color: #1e293b;
  margin-bottom: 8px;
}

.empty-state-modern p {
  font-size: 0.85rem;
  color: #64748b;
}

/* پیگیری */
.tracking-card-modern {
  background: #f8fafc;
  border-radius: 20px;
  padding: 24px;
}

.tracking-input-group {
  display: flex;
  gap: 12px;
}

.tracking-input-group .input-modern {
  flex: 1;
}

.btn-search {
  padding: 12px 24px;
  background: linear-gradient(135deg, #3f51b5, #2c3e8f);
  border: none;
  border-radius: 40px;
  color: white;
  font-size: 0.85rem;
  font-weight: 500;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

.tracking-result {
  margin-top: 24px;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid #e2e8f0;
}

.timeline-modern {
  padding-right: 20px;
}

.timeline-step-modern {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
  position: relative;
}

.timeline-step-modern:not(:last-child)::before {
  content: '';
  position: absolute;
  right: 23px;
  top: 40px;
  bottom: -24px;
  width: 2px;
  background: #e2e8f0;
}

.timeline-step-modern.completed:not(:last-child)::before {
  background: #10b981;
}

.timeline-icon {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  background: #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.timeline-step-modern.completed .timeline-icon {
  background: #10b981;
  color: white;
}

.timeline-step-modern.active .timeline-icon {
  background: #3f51b5;
  color: white;
  box-shadow: 0 0 0 4px rgba(63, 81, 181, 0.2);
}

.timeline-content {
  flex: 1;
}

.timeline-title {
  font-weight: 700;
  font-size: 0.9rem;
  color: #1e293b;
}

.timeline-date {
  font-size: 0.7rem;
  color: #94a3b8;
  margin: 4px 0;
}

.timeline-desc {
  font-size: 0.8rem;
  color: #64748b;
}

.status-details {
  background: white;
  border-radius: 16px;
  padding: 16px;
  margin-top: 20px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid #f1f5f9;
}

.detail-row:last-child {
  border-bottom: none;
}

/* قوانین */
.rules-card-modern {
  background: #f8fafc;
  border-radius: 20px;
  padding: 24px;
}

.rules-section {
  margin-bottom: 24px;
}

.rules-section h6 {
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 12px;
}

.rules-section ul {
  padding-right: 20px;
  color: #475569;
  font-size: 0.85rem;
}

.rules-section li {
  margin-bottom: 8px;
}

.time-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 12px;
}

.time-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 12px;
  background: white;
  border-radius: 12px;
  font-size: 0.85rem;
}

/* مودال */
.modal-glass-modern ::v-deep .modal-content {
  border-radius: 28px;
  overflow: hidden;
}

.modal-body-custom {
  padding: 24px;
}

.modal-header-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.modal-tracking-code {
  background: #f1f5f9;
  padding: 12px 16px;
  border-radius: 12px;
  margin-bottom: 20px;
  display: flex;
  justify-content: space-between;
}

.modal-section {
  margin-bottom: 20px;
}

.modal-section label {
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
  display: block;
}

.modal-footer-actions {
  display: flex;
  justify-content: flex-end;
}

/* مودال موفقیت */
.success-modal-content {
  text-align: center;
  padding: 20px;
}

.success-icon svg {
  width: 64px;
  height: 64px;
}

.success-modal-content h5 {
  margin: 16px 0 8px;
}

.tracking-code-box {
  background: #f1f5f9;
  padding: 12px;
  border-radius: 12px;
  font-family: monospace;
  font-size: 1.1rem;
  font-weight: bold;
  margin: 16px 0;
}

/* استایل نمایش فایل‌های ضمیمه در مودال */
.documents-list-modal {
  max-height: 300px;
  overflow-y: auto;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  background: #f8fafc;
}

.document-item-modal {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  border-bottom: 1px solid #e2e8f0;
  transition: all 0.2s;
}

.document-item-modal:last-child {
  border-bottom: none;
}

.document-item-modal:hover {
  background: white;
}

.document-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
  min-width: 0;
}

.document-info svg {
  flex-shrink: 0;
  color: #64748b;
}

.doc-name {
  font-size: 0.85rem;
  color: #1e293b;
  word-break: break-all;
  flex: 1;
}

.doc-size {
  font-size: 0.7rem;
  color: #94a3b8;
  flex-shrink: 0;
}

.document-actions {
  display: flex;
  gap: 8px;
}

.btn-download,
.btn-view {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  border-radius: 30px;
  font-size: 0.7rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
}

.btn-download {
  background: #e0e7ff;
  color: #3f51b5;
}

.btn-download:hover {
  background: #3f51b5;
  color: white;
  transform: translateY(-2px);
}

.btn-view {
  background: #e2e8f0;
  color: #475569;
}

.btn-view:hover {
  background: #1e293b;
  color: white;
  transform: translateY(-2px);
}

.no-documents {
  text-align: center;
  padding: 30px;
  background: #f8fafc;
  border-radius: 16px;
  border: 1px dashed #cbd5e1;
}

.no-documents svg {
  margin-bottom: 10px;
}

.no-documents span {
  display: block;
  font-size: 0.8rem;
  color: #94a3b8;
}

/* موبایل */
@media (max-width: 768px) {
  .page-spacer {
    margin-top: 60px;
  }

  .tab-header {
    flex-wrap: wrap;
    gap: 4px;
  }

  .tab-btn {
    padding: 10px 16px;
  }

  .tab-content-modern {
    padding: 20px;
  }

  .form-actions {
    flex-direction: column;
  }

  .btn-outline-modern,
  .btn-primary-modern {
    justify-content: center;
  }

  .tracking-input-group {
    flex-direction: column;
  }

  .complaint-card-header {
    flex-direction: column;
  }

  .file-limits-info {
    flex-direction: column;
  }
}
</style>