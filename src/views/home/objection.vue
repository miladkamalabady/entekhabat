<!-- views/UserComplaint.vue -->
<template>
  <div class="user-complaint-page">
    <!-- Header -->
    <b-container fluid class="page-header py-4">
      <b-row class="align-items-center">
        <b-col cols="12">
          <div class="d-flex align-items-center">
            <div class="complaint-icon mr-3">
              <b-icon icon="exclamation-triangle-fill"></b-icon>
            </div>
            <div>
              <h2 class="mb-1">اعتراض به تصمیم هیأت نظارت</h2>
              <p class="text-muted mb-0">فرم ثبت اعتراض به تصمیمات هیأت نظارت انتخابات</p>
            </div>
          </div>
        </b-col>
      </b-row>
    </b-container>

    <!-- Main Content -->
    <b-container class="complaint-container">
      <!-- User Info Card -->
      <b-card class="user-info-card mb-4">
        <b-row class="align-items-center">
          <b-col md="3" class="text-center mb-3 mb-md-0">
            <img
              :src="userInfo.photo || '/assets/img/avatars/imagen1.png'"
              class="user-avatar"
              alt="تصویر کاربر"
            />
          </b-col>
          <b-col md="9">
            <h4>{{ userInfo.name }}</h4>
            <div class="user-details">
              <div class="detail-item">
                <b-icon icon="card-text" class="ml-1"></b-icon>
                <strong>کد ملی:</strong>
                <span>{{ userInfo.nationalCode }}</span>
              </div>
              <div class="detail-item">
                <b-icon icon="phone" class="ml-1"></b-icon>
                <strong>شماره تماس:</strong>
                <span>{{ userInfo.phone }}</span>
              </div>
              <div class="detail-item">
                <b-icon icon="envelope" class="ml-1"></b-icon>
                <strong>ایمیل:</strong>
                <span>{{ userInfo.email }}</span>
              </div>
              <div class="detail-item">
                <b-icon icon="building" class="ml-1"></b-icon>
                <strong>مرکز آموزشی:</strong>
                <span>{{ userInfo.educationalCenter }}</span>
              </div>
            </div>
          </b-col>
        </b-row>
      </b-card>

      <!-- Tabs Navigation -->
      <b-card no-body class="mb-4">
        <b-tabs card v-model="activeTab">
          <!-- Tab 1: Submit New Complaint -->
          <b-tab title="ثبت اعتراض جدید" active>
            <div class="p-3">
              <!-- Decision Info -->
              <b-card class="mb-4">
                <h5 class="mb-3">مشخصات تصمیم مورد اعتراض</h5>
                <b-form @submit.prevent="validateBeforeSubmit">
                  <b-row>
                    <b-col md="6">
                      <b-form-group
                        label="نوع تصمیم"
                        label-for="decision-type"
                        :state="formState.decisionType"
                        invalid-feedback="لطفا نوع تصمیم را انتخاب کنید"
                      >
                        <b-form-select
                          id="decision-type"
                          v-model="complaintData.decisionType"
                          :options="decisionTypeOptions"
                          required
                          :state="formState.decisionType"
                          @change="onDecisionTypeChange"
                        >
                          <template #first>
                            <b-form-select-option :value="null" disabled>
                              -- لطفا انتخاب کنید --
                            </b-form-select-option>
                          </template>
                        </b-form-select>
                      </b-form-group>
                    </b-col>

                    <b-col md="6">
                      <b-form-group
                        label="شماره تصمیم/پرونده"
                        label-for="case-number"
                        :state="formState.caseNumber"
                        invalid-feedback="لطفا شماره پرونده را وارد کنید"
                      >
                        <b-form-input
                          id="case-number"
                          v-model="complaintData.caseNumber"
                          placeholder="مثال: ۱۴۰۲/۱۱/۱۲۰"
                          required
                          :state="formState.caseNumber"
                        ></b-form-input>
                      </b-form-group>
                    </b-col>
                  </b-row>

                  <!-- Candidate Info (if applicable) -->
                  <div v-if="showCandidateFields" class="candidate-fields mt-4 pt-3 border-top">
                    <h6 class="mb-3">اطلاعات کاندیدا</h6>
                    <b-row>
                      <b-col md="4">
                        <b-form-group label="نام کاندیدا">
                          <b-form-input
                            v-model="complaintData.candidateName"
                            placeholder="نام کامل کاندیدا"
                          ></b-form-input>
                        </b-form-group>
                      </b-col>
                      <b-col md="4">
                        <b-form-group label="منطقه انتخابی">
                          <b-form-input
                            v-model="complaintData.candidateRegion"
                            placeholder="منطقه انتخابی"
                          ></b-form-input>
                        </b-form-group>
                      </b-col>
                      <b-col md="4">
                        <b-form-group label="مقام مورد نظر">
                          <b-form-input
                            v-model="complaintData.candidatePosition"
                            placeholder="مقام مورد درخواست"
                          ></b-form-input>
                        </b-form-group>
                      </b-col>
                    </b-row>
                  </div>

                  <!-- Complaint Details -->
                  <div class="complaint-details mt-4 pt-3 border-top">
                    <h6 class="mb-3">جزئیات اعتراض</h6>
                    
                    <b-form-group
                      label="موضوع اعتراض"
                      label-for="complaint-subject"
                      :state="formState.subject"
                      invalid-feedback="لطفا موضوع اعتراض را وارد کنید"
                    >
                      <b-form-input
                        id="complaint-subject"
                        v-model="complaintData.subject"
                        placeholder="موضوع اصلی اعتراض خود را وارد کنید"
                        required
                        :state="formState.subject"
                      ></b-form-input>
                    </b-form-group>

                    <b-form-group
                      label="شرح کامل اعتراض"
                      label-for="complaint-description"
                      :state="formState.description"
                      invalid-feedback="لطفا شرح اعتراض را با جزئیات وارد کنید"
                    >
                      <b-form-textarea
                        id="complaint-description"
                        v-model="complaintData.description"
                        placeholder="شرح کامل اعتراض خود را با ذکر دلایل و مستندات وارد کنید"
                        rows="6"
                        required
                        :state="formState.description"
                        :maxlength="2000"
                      ></b-form-textarea>
                      <small class="text-muted d-block mt-1">
                        {{ complaintData.description.length }} / 2000 کاراکتر
                      </small>
                    </b-form-group>

                    <b-form-group
                      label="دلایل اصلی اعتراض"
                      label-for="complaint-reasons"
                    >
                      <b-form-tags
                        v-model="complaintData.reasons"
                        placeholder="دلیل اعتراض را وارد و Enter بزنید"
                        tag-variant="primary"
                        separator=","
                        add-on-change
                        :limit="5"
                      ></b-form-tags>
                      <small class="text-muted d-block mt-1">
                        حداکثر ۵ دلیل مجاز است
                      </small>
                    </b-form-group>

                    <!-- Document Upload -->
                    <b-form-group label="ضمیمه مدارک">
                      <div class="document-upload-area">
                        <b-form-file
                          v-model="complaintData.documents"
                          multiple
                          :file-name-formatter="formatFileNames"
                          placeholder="فایل‌های خود را انتخاب کنید یا اینجا رها کنید"
                          drop-placeholder="فایل‌ها را اینجا رها کنید"
                          accept=".pdf,.jpg,.jpeg,.png,.doc,.docx"
                          :state="formState.documents"
                          @input="onDocumentsChange"
                        ></b-form-file>
                        
                        <small class="text-muted d-block mt-2">
                          فرمت‌های مجاز: PDF, JPG, PNG, DOC, DOCX | حداکثر حجم هر فایل: ۵ مگابایت
                        </small>

                        <!-- Uploaded Files List -->
                        <div v-if="uploadedFiles.length > 0" class="uploaded-files mt-3">
                          <div class="files-header">
                            <strong>فایل‌های انتخاب شده:</strong>
                            <small>{{ uploadedFiles.length }} فایل</small>
                          </div>
                          <div class="files-list">
                            <div
                              v-for="(file, index) in uploadedFiles"
                              :key="index"
                              class="file-item"
                            >
                              <div class="file-info">
                                <b-icon :icon="getFileIcon(file)" class="ml-2"></b-icon>
                                <span class="file-name">{{ file.name }}</span>
                                <small class="file-size">{{ formatFileSize(file.size) }}</small>
                              </div>
                              <div class="file-actions">
                                <b-button
                                  size="sm"
                                  variant="outline-danger"
                                  @click="removeFile(index)"
                                >
                                  <b-icon icon="trash"></b-icon>
                                </b-button>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </b-form-group>

                    <!-- Urgency Level -->
                    <b-form-group label="درجه فوریت">
                      <b-form-radio-group
                        v-model="complaintData.urgency"
                        :options="urgencyOptions"
                        buttons
                        button-variant="outline-primary"
                        size="sm"
                        name="urgency-buttons"
                      ></b-form-radio-group>
                    </b-form-group>

                    <!-- Declaration -->
                    <b-card class="declaration-card mt-4">
                      <b-form-checkbox
                        v-model="complaintData.declaration"
                        :state="formState.declaration"
                        required
                      >
                        <small>
                          <strong>تعهدنامه:</strong> 
                          من متعهد می‌شوم که اطلاعات فوق صحیح و مستند بوده و در صورت اثبات خلاف آن، 
                          مسئولیت حقوقی و قانونی آن را می‌پذیرم. همچنین موافقت می‌کنم که این اعتراض 
                          طبق آیین‌نامه هیأت نظارت بررسی شود.
                        </small>
                      </b-form-checkbox>
                      <b-form-invalid-feedback :state="formState.declaration">
                        لطفا این گزینه را تایید کنید
                      </b-form-invalid-feedback>
                    </b-card>

                    <!-- Submit Button -->
                    <div class="text-center mt-4">
                      <b-button
                        type="submit"
                        variant="primary"
                        size="lg"
                        :disabled="submitting"
                        class="submit-btn"
                      >
                        <b-icon icon="send-check" class="ml-1"></b-icon>
                        {{ submitting ? 'در حال ارسال...' : 'ثبت اعتراض' }}
                      </b-button>
                      
                      <b-button
                        variant="outline-secondary"
                        size="lg"
                        class="mr-3"
                        @click="resetForm"
                      >
                        <b-icon icon="arrow-clockwise" class="ml-1"></b-icon>
                        پاک کردن فرم
                      </b-button>
                    </div>
                  </div>
                </b-form>
              </b-card>
            </div>
          </b-tab>

          <!-- Tab 2: My Complaints -->
          <b-tab title="اعتراضات من">
            <div class="p-3">
              <!-- Complaints List -->
              <div v-if="userComplaints.length > 0">
                <div class="complaints-list">
                  <div
                    v-for="complaint in userComplaints"
                    :key="complaint.id"
                    class="complaint-item mb-3"
                  >
                    <b-card>
                      <!-- Complaint Header -->
                      <div class="complaint-header mb-3">
                        <div class="d-flex justify-content-between align-items-center flex-wrap">
                          <div class="complaint-title">
                            <h5 class="mb-1">{{ complaint.subject }}</h5>
                            <div class="complaint-meta">
                              <b-badge :variant="getComplaintStatusVariant(complaint.status)" class="ml-2">
                                {{ getComplaintStatusText(complaint.status) }}
                              </b-badge>
                              <small class="text-muted">
                                <b-icon icon="calendar" class="ml-1"></b-icon>
                                {{ complaint.submittedDate }}
                              </small>
                              <small class="text-muted mr-3">
                                <b-icon icon="hash" class="ml-1"></b-icon>
                                شماره: {{ complaint.trackingCode }}
                              </small>
                            </div>
                          </div>
                          <div class="complaint-actions">
                            <b-button-group size="sm">
                              <b-button
                                variant="outline-info"
                                @click="viewComplaintDetails(complaint)"
                              >
                                <b-icon icon="eye"></b-icon>
                                مشاهده
                              </b-button>
                              <b-button
                                variant="outline-secondary"
                                v-if="complaint.status === 'pending'"
                                @click="editComplaint(complaint)"
                              >
                                <b-icon icon="pencil"></b-icon>
                                ویرایش
                              </b-button>
                              <b-button
                                variant="outline-danger"
                                v-if="complaint.status === 'pending'"
                                @click="cancelComplaint(complaint)"
                              >
                                <b-icon icon="x-circle"></b-icon>
                                لغو
                              </b-button>
                            </b-button-group>
                          </div>
                        </div>
                      </div>

                      <!-- Complaint Preview -->
                      <div class="complaint-preview">
                        <p class="text-muted mb-2">
                          {{ complaint.preview }}
                        </p>
                        <div class="preview-footer">
                          <small>
                            <b-icon icon="clock-history" class="ml-1"></b-icon>
                            آخرین به‌روزرسانی: {{ complaint.lastUpdate }}
                          </small>
                          <small class="mr-3">
                            <b-icon icon="file-earmark" class="ml-1"></b-icon>
                            {{ complaint.documentsCount }} سند ضمیمه
                          </small>
                        </div>
                      </div>
                    </b-card>
                  </div>
                </div>

                <!-- Pagination -->
                <div class="text-center mt-4" v-if="totalPages > 1">
                  <b-pagination
                    v-model="currentPage"
                    :total-rows="totalComplaints"
                    :per-page="complaintsPerPage"
                    align="center"
                    size="sm"
                  ></b-pagination>
                </div>
              </div>

              <!-- No Complaints -->
              <div v-else class="text-center py-5">
                <b-icon icon="inbox" font-scale="4" variant="secondary"></b-icon>
                <h5 class="mt-3">هیچ اعتراضی ثبت نکرده‌اید</h5>
                <p class="text-muted">برای ثبت اعتراض جدید، به تب "ثبت اعتراض جدید" مراجعه کنید.</p>
              </div>
            </div>
          </b-tab>

          <!-- Tab 3: Tracking -->
          <b-tab title="پیگیری اعتراض">
            <div class="p-3">
              <b-card>
                <h5 class="mb-4">پیگیری وضعیت اعتراض</h5>
                
                <!-- Tracking Form -->
                <b-form @submit.prevent="trackComplaint" class="tracking-form">
                  <b-row class="align-items-end">
                    <b-col md="8">
                      <b-form-group
                        label="شماره پیگیری (کد رهگیری)"
                        label-for="tracking-code"
                      >
                        <b-form-input
                          id="tracking-code"
                          v-model="trackingCode"
                          placeholder="کد ۱۲ رقمی پیگیری خود را وارد کنید"
                          :state="trackingState"
                          required
                        ></b-form-input>
                        <b-form-invalid-feedback :state="trackingState">
                          کد پیگیری باید ۱۲ رقم باشد
                        </b-form-invalid-feedback>
                        <b-form-valid-feedback :state="trackingState">
                          کد پیگیری معتبر است
                        </b-form-valid-feedback>
                      </b-form-group>
                    </b-col>
                    <b-col md="4">
                      <b-button
                        type="submit"
                        variant="info"
                        class="w-100"
                        :disabled="trackingLoading"
                      >
                        <b-icon icon="search" class="ml-1"></b-icon>
                        {{ trackingLoading ? 'در حال جستجو...' : 'پیگیری' }}
                      </b-button>
                    </b-col>
                  </b-row>
                </b-form>

                <!-- Tracking Result -->
                <div v-if="trackingResult" class="tracking-result mt-4">
                  <div class="tracking-header">
                    <h6 class="mb-3">وضعیت اعتراض</h6>
                  </div>

                  <!-- Timeline -->
                  <div class="timeline">
                    <div
                      v-for="(step, index) in trackingResult.timeline"
                      :key="index"
                      class="timeline-step"
                      :class="{ active: step.active, completed: step.completed }"
                    >
                      <div class="timeline-icon">
                        <b-icon :icon="step.icon"></b-icon>
                      </div>
                      <div class="timeline-content">
                        <div class="timeline-title">{{ step.title }}</div>
                        <div class="timeline-date">{{ step.date }}</div>
                        <div v-if="step.description" class="timeline-description">
                          {{ step.description }}
                        </div>
                        <div v-if="step.officer" class="timeline-officer">
                          <small>
                            <b-icon icon="person" class="ml-1"></b-icon>
                            {{ step.officer }}
                          </small>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Current Status -->
                  <div class="current-status mt-4">
                    <b-card>
                      <div class="status-summary">
                        <div class="status-header">
                          <h6>خلاصه وضعیت</h6>
                          <b-badge :variant="getComplaintStatusVariant(trackingResult.status)">
                            {{ getComplaintStatusText(trackingResult.status) }}
                          </b-badge>
                        </div>
                        <div class="status-details">
                          <div class="detail-row">
                            <strong>موضوع:</strong>
                            <span>{{ trackingResult.subject }}</span>
                          </div>
                          <div class="detail-row">
                            <strong>تاریخ ثبت:</strong>
                            <span>{{ trackingResult.submittedDate }}</span>
                          </div>
                          <div class="detail-row">
                            <strong>آخرین به‌روزرسانی:</strong>
                            <span>{{ trackingResult.lastUpdate }}</span>
                          </div>
                          <div class="detail-row" v-if="trackingResult.assignedTo">
                            <strong>کارشناس رسیدگی:</strong>
                            <span>{{ trackingResult.assignedTo }}</span>
                          </div>
                          <div class="detail-row" v-if="trackingResult.estimatedDate">
                            <strong>تخمین زمان پاسخ:</strong>
                            <span>{{ trackingResult.estimatedDate }}</span>
                          </div>
                        </div>
                      </div>
                    </b-card>
                  </div>

                  <!-- Download Documents -->
                  <div v-if="trackingResult.documents && trackingResult.documents.length > 0" class="documents-section mt-4">
                    <h6 class="mb-3">مدارک و پاسخ‌ها</h6>
                    <b-card>
                      <div class="documents-list">
                        <div
                          v-for="doc in trackingResult.documents"
                          :key="doc.id"
                          class="document-item"
                        >
                          <div class="d-flex justify-content-between align-items-center">
                            <div class="document-info">
                              <b-icon :icon="getFileIcon(doc)" class="ml-2"></b-icon>
                              <span>{{ doc.name }}</span>
                              <small class="text-muted mr-2">{{ doc.type }}</small>
                              <small class="text-muted">{{ doc.date }}</small>
                            </div>
                            <div class="document-actions">
                              <b-button
                                size="sm"
                                variant="outline-primary"
                                :href="doc.url"
                                target="_blank"
                              >
                                <b-icon icon="eye"></b-icon>
                                مشاهده
                              </b-button>
                              <b-button
                                size="sm"
                                variant="outline-success"
                                :href="doc.url"
                                download
                                class="mr-2"
                              >
                                <b-icon icon="download"></b-icon>
                                دانلود
                              </b-button>
                            </div>
                          </div>
                        </div>
                      </div>
                    </b-card>
                  </div>

                  <!-- Add Comment (if status is pending) -->
                  <div v-if="trackingResult.status === 'under_review'" class="add-comment mt-4">
                    <h6 class="mb-3">ارسال پیام پیگیری</h6>
                    <b-form @submit.prevent="submitFollowUp">
                      <b-form-group>
                        <b-form-textarea
                          v-model="followUpMessage"
                          rows="3"
                          placeholder="پیام خود را برای پیگیری بیشتر وارد کنید"
                          :maxlength="500"
                        ></b-form-textarea>
                        <small class="text-muted d-block mt-1">
                          {{ followUpMessage.length }} / 500 کاراکتر
                        </small>
                      </b-form-group>
                      <div class="text-left">
                        <b-button
                          type="submit"
                          variant="primary"
                          size="sm"
                          :disabled="!followUpMessage.trim()"
                        >
                          <b-icon icon="send" class="ml-1"></b-icon>
                          ارسال پیام
                        </b-button>
                      </div>
                    </b-form>
                  </div>
                </div>
              </b-card>
            </div>
          </b-tab>

          <!-- Tab 4: Rules and Instructions -->
          <b-tab title="قوانین و راهنما">
            <div class="p-3">
              <b-card>
                <h5 class="mb-4">راهنمای ثبت اعتراض</h5>
                
                <div class="rules-content">
                  <!-- Introduction -->
                  <div class="rules-section mb-4">
                    <h6 class="section-title">
                      <b-icon icon="info-circle" class="ml-2"></b-icon>
                      مقدمه
                    </h6>
                    <p>
                      سیستم اعتراضات هیأت نظارت انتخابات، بستری شفاف و قانونی برای رسیدگی به 
                      اعتراضات مربوط به تصمیمات هیأت نظارت فراهم می‌کند. این سیستم مطابق با 
                      آیین‌نامه داخلی هیأت نظارت و قوانین جاری کشور طراحی شده است.
                    </p>
                  </div>

                  <!-- Conditions -->
                  <div class="rules-section mb-4">
                    <h6 class="section-title">
                      <b-icon icon="check-circle" class="ml-2"></b-icon>
                      شرایط و ضوابط
                    </h6>
                    <ul class="rules-list">
                      <li>اعتراض باید حداکثر تا ۷ روز پس از اعلام تصمیم هیأت نظارت ثبت شود.</li>
                      <li>اعتراض باید مستند و همراه با دلایل محکمه‌پسند ارائه شود.</li>
                      <li>اعتراضات توهین‌آمیز یا فاقد مستندات معتبر بررسی نخواهند شد.</li>
                      <li>اعتراض‌دهنده باید متعهد شود که اطلاعات ارائه شده صحیح می‌باشد.</li>
                      <li>اعتراضات مربوط به امور اداری باید از طریق کانال‌های مربوطه پیگیری شود.</li>
                    </ul>
                  </div>

                  <!-- Process Steps -->
                  <div class="rules-section mb-4">
                    <h6 class="section-title">
                      <b-icon icon="arrow-repeat" class="ml-2"></b-icon>
                      مراحل رسیدگی
                    </h6>
                    <div class="process-steps">
                      <div class="step">
                        <div class="step-number">۱</div>
                        <div class="step-content">
                          <strong>ثبت اعتراض:</strong> تکمیل فرم و ارسال مدارک
                        </div>
                      </div>
                      <div class="step">
                        <div class="step-number">۲</div>
                        <div class="step-content">
                          <strong>بررسی اولیه:</strong> بررسی صحت و تکمیل مدارک
                        </div>
                      </div>
                      <div class="step">
                        <div class="step-number">۳</div>
                        <div class="step-content">
                          <strong>ارجاع به کارشناس:</strong> ارجاع به کارشناس مربوطه
                        </div>
                      </div>
                      <div class="step">
                        <div class="step-number">۴</div>
                        <div class="step-content">
                          <strong>بررسی تخصصی:</strong> بررسی دقیق اعتراض و مستندات
                        </div>
                      </div>
                      <div class="step">
                        <div class="step-number">۵</div>
                        <div class="step-content">
                          <strong>صدور رأی:</strong> تصمیم‌گیری نهایی توسط هیأت
                        </div>
                      </div>
                      <div class="step">
                        <div class="step-number">۶</div>
                        <div class="step-content">
                          <strong>اعلام نتیجه:</strong> اعلام نتیجه به اعتراض‌دهنده
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- Time Limits -->
                  <div class="rules-section mb-4">
                    <h6 class="section-title">
                      <b-icon icon="clock" class="ml-2"></b-icon>
                      مهلت‌های قانونی
                    </h6>
                    <div class="time-limits">
                      <div class="time-item">
                        <div class="time-label">ثبت اعتراض:</div>
                        <div class="time-value">۷ روز کاری</div>
                      </div>
                      <div class="time-item">
                        <div class="time-label">بررسی اولیه:</div>
                        <div class="time-value">۳ روز کاری</div>
                      </div>
                      <div class="time-item">
                        <div class="time-label">بررسی تخصصی:</div>
                        <div class="time-value">۱۴ روز کاری</div>
                      </div>
                      <div class="time-item">
                        <div class="time-label">صدور رأی:</div>
                        <div class="time-value">۷ روز کاری</div>
                      </div>
                      <div class="time-item">
                        <div class="time-label">کل فرآیند:</div>
                        <div class="time-value">۳۱ روز کاری</div>
                      </div>
                    </div>
                  </div>

                  <!-- Contact Info -->
                  <div class="rules-section">
                    <h6 class="section-title">
                      <b-icon icon="telephone" class="ml-2"></b-icon>
                      راه‌های ارتباطی
                    </h6>
                    <div class="contact-info">
                      <div class="contact-item">
                        <b-icon icon="telephone-fill"></b-icon>
                        <span>تلفن تماس: ۰۲۱-۸۸۵۶۰۰۰۰</span>
                      </div>
                      <div class="contact-item">
                        <b-icon icon="envelope-fill"></b-icon>
                        <span>ایمیل: complaint@election-supervision.ir</span>
                      </div>
                      <div class="contact-item">
                        <b-icon icon="geo-alt-fill"></b-icon>
                        <span>آدرس: تهران، میدان فردوسی، خیابان ایرانشهر، هیأت نظارت انتخابات</span>
                      </div>
                      <div class="contact-item">
                        <b-icon icon="clock-fill"></b-icon>
                        <span>ساعات کاری: شنبه تا چهارشنبه، ۸:۰۰ تا ۱۶:۰۰</span>
                      </div>
                    </div>
                  </div>
                </div>
              </b-card>
            </div>
          </b-tab>
        </b-tabs>
      </b-card>
    </b-container>

    <!-- Complaint Details Modal -->
    <b-modal
      v-model="showComplaintModal"
      :title="`جزئیات اعتراض - ${selectedComplaint?.trackingCode || ''}`"
      size="lg"
      hide-footer
      centered
      scrollable
    >
      <div v-if="selectedComplaint" class="complaint-details-modal">
        <!-- Complaint Header -->
        <div class="complaint-header mb-4">
          <div class="d-flex justify-content-between align-items-start">
            <div>
              <h5>{{ selectedComplaint.subject }}</h5>
              <div class="complaint-meta">
                <b-badge :variant="getComplaintStatusVariant(selectedComplaint.status)" class="ml-2">
                  {{ getComplaintStatusText(selectedComplaint.status) }}
                </b-badge>
                <small class="text-muted mr-3">
                  <b-icon icon="calendar" class="ml-1"></b-icon>
                  تاریخ ثبت: {{ selectedComplaint.submittedDate }}
                </small>
              </div>
            </div>
            <div class="text-left">
              <small class="text-muted d-block">کد پیگیری:</small>
              <strong class="tracking-code">{{ selectedComplaint.trackingCode }}</strong>
            </div>
          </div>
        </div>

        <!-- Complaint Body -->
        <div class="complaint-body">
          <!-- Basic Info -->
          <div class="basic-info mb-4">
            <h6 class="section-title mb-3">اطلاعات کلی</h6>
            <b-row>
              <b-col md="6">
                <div class="info-item">
                  <strong>نوع تصمیم:</strong>
                  <span>{{ selectedComplaint.decisionType }}</span>
                </div>
                <div class="info-item">
                  <strong>شماره پرونده:</strong>
                  <span>{{ selectedComplaint.caseNumber }}</span>
                </div>
              </b-col>
              <b-col md="6">
                <div class="info-item">
                  <strong>درجه فوریت:</strong>
                  <span>{{ selectedComplaint.urgency }}</span>
                </div>
                <div class="info-item">
                  <strong>تعداد مدارک:</strong>
                  <span>{{ selectedComplaint.documentsCount }}</span>
                </div>
              </b-col>
            </b-row>
          </div>

          <!-- Description -->
          <div class="description mb-4">
            <h6 class="section-title mb-3">شرح اعتراض</h6>
            <div class="description-content">
              {{ selectedComplaint.description }}
            </div>
          </div>

          <!-- Reasons -->
          <div class="reasons mb-4" v-if="selectedComplaint.reasons && selectedComplaint.reasons.length > 0">
            <h6 class="section-title mb-3">دلایل اعتراض</h6>
            <div class="reasons-list">
              <b-badge
                v-for="(reason, index) in selectedComplaint.reasons"
                :key="index"
                variant="primary"
                class="mr-2 mb-2"
              >
                {{ reason }}
              </b-badge>
            </div>
          </div>

          <!-- Timeline -->
          <div class="timeline-section mb-4">
            <h6 class="section-title mb-3">سیر رسیدگی</h6>
            <div class="simple-timeline">
              <div
                v-for="(step, index) in selectedComplaint.timeline"
                :key="index"
                class="timeline-item"
              >
                <div class="timeline-dot"></div>
                <div class="timeline-content">
                  <div class="timeline-title">{{ step.title }}</div>
                  <div class="timeline-date">{{ step.date }}</div>
                  <div v-if="step.description" class="timeline-description">
                    {{ step.description }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Documents -->
          <div class="documents-section mb-4" v-if="selectedComplaint.documents && selectedComplaint.documents.length > 0">
            <h6 class="section-title mb-3">مدارک ضمیمه</h6>
            <b-card>
              <div class="documents-list">
                <div
                  v-for="doc in selectedComplaint.documents"
                  :key="doc.id"
                  class="document-item"
                >
                  <div class="d-flex justify-content-between align-items-center">
                    <div class="document-info">
                      <b-icon :icon="getFileIcon(doc)" class="ml-2"></b-icon>
                      <span>{{ doc.name }}</span>
                      <small class="text-muted mr-2">{{ formatFileSize(doc.size) }}</small>
                    </div>
                    <div class="document-actions">
                      <b-button
                        size="sm"
                        variant="outline-primary"
                        :href="doc.url"
                        target="_blank"
                      >
                        <b-icon icon="eye"></b-icon>
                        مشاهده
                      </b-button>
                      <b-button
                        size="sm"
                        variant="outline-success"
                        :href="doc.url"
                        download
                        class="mr-2"
                      >
                        <b-icon icon="download"></b-icon>
                        دانلود
                      </b-button>
                    </div>
                  </div>
                </div>
              </div>
            </b-card>
          </div>

          <!-- Response (if available) -->
          <div class="response-section" v-if="selectedComplaint.response">
            <h6 class="section-title mb-3">
              <b-icon icon="chat-left-text" class="ml-2"></b-icon>
              پاسخ هیأت نظارت
            </h6>
            <b-alert :variant="selectedComplaint.status === 'approved' ? 'success' : 'warning'" show>
              <div class="response-header mb-3">
                <div class="d-flex justify-content-between">
                  <strong>نتیجه بررسی:</strong>
                  <small>{{ selectedComplaint.responseDate }}</small>
                </div>
              </div>
              <div class="response-content">
                {{ selectedComplaint.response }}
              </div>
              <div v-if="selectedComplaint.responseOfficer" class="response-officer mt-3">
                <small>
                  <b-icon icon="person" class="ml-1"></b-icon>
                  امضاء: {{ selectedComplaint.responseOfficer }}
                </small>
              </div>
            </b-alert>
          </div>
        </div>
      </div>
    </b-modal>

    <!-- Success Modal -->
    <b-modal
      v-model="showSuccessModal"
      title="ثبت اعتراض با موفقیت انجام شد"
      hide-footer
      centered
      no-close-on-backdrop
      hide-header-close
    >
      <div class="success-modal text-center">
        <div class="success-icon mb-3">
          <b-icon icon="check-circle-fill" font-scale="4" variant="success"></b-icon>
        </div>
        <h5 class="mb-3">اعتراض شما با موفقیت ثبت شد</h5>
        <p class="text-muted mb-4">
          اعتراض شما دریافت و در سیستم ثبت گردید. کد پیگیری زیر را برای پیگیری حفظ کنید.
        </p>
        <div class="tracking-code-display mb-4">
          <strong>کد پیگیری:</strong>
          <div class="code">{{ newTrackingCode }}</div>
          <small class="text-muted">این کد را در قسمت پیگیری وارد کنید</small>
        </div>
        <div class="actions">
          <b-button
            variant="primary"
            @click="printComplaint"
            class="mr-2"
          >
            <b-icon icon="printer" class="ml-1"></b-icon>
            چاپ رسید
          </b-button>
          <b-button
            variant="outline-secondary"
            @click="showSuccessModal = false"
          >
            بازگشت
          </b-button>
        </div>
      </div>
    </b-modal>

    <!-- Footer -->
    <footer class="complaint-footer">
      <b-container>
        <div class="text-center py-3">
          <small class="text-muted">
            © ۱۴۰۲ - سیستم اعتراضات هیأت نظارت انتخابات صندوق ذخیره فرهنگیان
          </small>
          <div class="footer-links mt-2">
            <a href="#" @click.prevent="downloadGuide" class="ml-3">
              <b-icon icon="download" class="ml-1"></b-icon>
              دانلود راهنما
            </a>
            <a href="#" @click.prevent="showFAQ" class="ml-3">
              <b-icon icon="question-circle" class="ml-1"></b-icon>
              سوالات متداول
            </a>
            <a href="tel:02188560000" class="ml-3">
              <b-icon icon="telephone" class="ml-1"></b-icon>
              پشتیبانی
            </a>
          </div>
        </div>
      </b-container>
    </footer>
  </div>
</template>

<script>
export default {
  name: 'UserComplaint',
  data() {
    return {
      activeTab: 0,
      submitting: false,
      trackingLoading: false,
      
      // User Information
      userInfo: {
        name: 'دکتر محمدرضا احمدی',
        nationalCode: '۰۰۹۴۵۶۷۸۹۰',
        phone: '۰۹۱۲۳۴۵۶۷۸۹',
        email: 'm.ahmadi@example.com',
        educationalCenter: 'دانشگاه تهران - دانشکده علوم تربیتی',
        photo: '/assets/img/avatars/imagen1.png'
      },
      
      // Complaint Form Data
      complaintData: {
        decisionType: null,
        caseNumber: '',
        candidateName: '',
        candidateRegion: '',
        candidatePosition: '',
        subject: '',
        description: '',
        reasons: [],
        documents: [],
        urgency: 'normal',
        declaration: false
      },
      
      // Form Validation States
      formState: {
        decisionType: null,
        caseNumber: null,
        subject: null,
        description: null,
        documents: null,
        declaration: null
      },
      
      // Options
      decisionTypeOptions: [
        { value: 'candidate_rejection', text: 'رد صلاحیت کاندیداتوری' },
        { value: 'document_rejection', text: 'رد مدارک' },
        { value: 'advertisement_rejection', text: 'رد تبلیغات' },
        { value: 'voting_irregularity', text: 'تخلفات انتخاباتی' },
        { value: 'procedure_violation', text: 'تخلفات آیین‌نامه‌ای' },
        { value: 'other', text: 'سایر موارد' }
      ],
      
      urgencyOptions: [
        { value: 'low', text: 'کم' },
        { value: 'normal', text: 'متوسط' },
        { value: 'high', text: 'زیاد' },
        { value: 'urgent', text: 'فوری' }
      ],
      
      // Uploaded Files
      uploadedFiles: [],
      
      // User Complaints
      userComplaints: [],
      currentPage: 1,
      totalPages: 1,
      totalComplaints: 0,
      complaintsPerPage: 10,
      
      // Tracking
      trackingCode: '',
      trackingResult: null,
      trackingState: null,
      followUpMessage: '',
      
      // Modals
      showComplaintModal: false,
      showSuccessModal: false,
      selectedComplaint: null,
      newTrackingCode: ''
    }
  },
  computed: {
    showCandidateFields() {
      return this.complaintData.decisionType === 'candidate_rejection' || 
             this.complaintData.decisionType === 'document_rejection'
    },
    
    // Sample data for user complaints (in real app, fetch from API)
    sampleComplaints() {
      return [
        {
          id: 1,
          trackingCode: '۱۴۰۲/۱۱/۱۰۰۱',
          subject: 'اعتراض به رد صلاحیت کاندیداتوری',
          status: 'pending',
          submittedDate: '۱۴۰۲/۱۱/۱۰',
          lastUpdate: '۱۴۰۲/۱۱/۱۲',
          preview: 'با توجه به مستندات ارائه شده، درخواست تجدید نظر در تصمیم هیأت نظارت دارم...',
          documentsCount: 3,
          decisionType: 'candidate_rejection',
          caseNumber: '۱۴۰۲/۱۱/۱۲۰',
          description: 'شرح کامل اعتراض در مورد رد صلاحیت کاندیداتوری...',
          reasons: ['عدم بررسی دقیق مدارک', 'تفسیر نادرست آیین‌نامه'],
          documents: [
            { id: 1, name: 'مدرک تحصیلی.pdf', size: 2048576, url: '#', type: 'PDF' }
          ],
          urgency: 'high',
          timeline: [
            { title: 'ثبت اعتراض', date: '۱۴۰۲/۱۱/۱۰', description: 'اعتراض توسط کاربر ثبت شد' },
            { title: 'بررسی اولیه', date: '۱۴۰۲/۱۱/۱۱', description: 'مدارک مورد بررسی قرار گرفت' }
          ]
        },
        {
          id: 2,
          trackingCode: '۱۴۰۲/۱۱/۰۹۹۸',
          subject: 'اعتراض به رد تبلیغات انتخاباتی',
          status: 'under_review',
          submittedDate: '۱۴۰۲/۱۱/۰۵',
          lastUpdate: '۱۴۰۲/۱۱/۰۹',
          preview: 'تبلیغات انتخاباتی مطابق با آیین‌نامه بوده و دلیلی برای رد آن وجود ندارد...',
          documentsCount: 2,
          decisionType: 'advertisement_rejection',
          caseNumber: '۱۴۰۲/۱۱/۱۱۵'
        },
        {
          id: 3,
          trackingCode: '۱۴۰۲/۱۱/۰۸۹۵',
          subject: 'اعتراض به تخلفات انتخاباتی',
          status: 'approved',
          submittedDate: '۱۴۰۲/۱۰/۲۸',
          lastUpdate: '۱۴۰۲/۱۱/۰۸',
          preview: 'بررسی اعتراض تکمیل و درخواست شما پذیرفته شد...',
          documentsCount: 5,
          decisionType: 'voting_irregularity',
          caseNumber: '۱۴۰۲/۱۱/۱۱۰',
          response: 'با توجه به مدارک ارائه شده، اعتراض شما وارد تشخیص داده شد. دستورات لازم برای رسیدگی صادر گردید.',
          responseDate: '۱۴۰۲/۱۱/۰۸',
          responseOfficer: 'دکتر سید حسن موسوی'
        }
      ]
    }
  },
  mounted() {
    this.loadUserComplaints()
  },
  methods: {
    // Form Methods
    onDecisionTypeChange() {
      this.formState.decisionType = this.complaintData.decisionType ? true : false
    },
    
    formatFileNames(files) {
      return files.length === 1 ? files[0].name : `${files.length} فایل انتخاب شده`
    },
    
    onDocumentsChange(files) {
      this.uploadedFiles = Array.from(files)
      this.formState.documents = files.length > 0 ? true : null
    },
    
    removeFile(index) {
      this.uploadedFiles.splice(index, 1)
      // Also remove from complaintData.documents
      if (this.complaintData.documents[index]) {
        this.complaintData.documents.splice(index, 1)
      }
    },
    
    getFileIcon(file) {
      if (file.type) {
        if (file.type.includes('pdf')) return 'file-earmark-pdf'
        if (file.type.includes('image')) return 'file-earmark-image'
        if (file.type.includes('word') || file.type.includes('document')) return 'file-earmark-word'
      }
      
      const extension = file.name.split('.').pop().toLowerCase()
      switch (extension) {
        case 'pdf': return 'file-earmark-pdf'
        case 'jpg':
        case 'jpeg':
        case 'png':
        case 'gif': return 'file-earmark-image'
        case 'doc':
        case 'docx': return 'file-earmark-word'
        default: return 'file-earmark'
      }
    },
    
    formatFileSize(bytes) {
      if (bytes === 0) return '0 بایت'
      const k = 1024
      const sizes = ['بایت', 'کیلوبایت', 'مگابایت', 'گیگابایت']
      const i = Math.floor(Math.log(bytes) / Math.log(k))
      return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i]
    },
    
    validateBeforeSubmit() {
      // Reset validation states
      Object.keys(this.formState).forEach(key => {
        this.formState[key] = null
      })
      
      // Validate required fields
      let isValid = true
      
      if (!this.complaintData.decisionType) {
        this.formState.decisionType = false
        isValid = false
      }
      
      if (!this.complaintData.caseNumber.trim()) {
        this.formState.caseNumber = false
        isValid = false
      }
      
      if (!this.complaintData.subject.trim()) {
        this.formState.subject = false
        isValid = false
      }
      
      if (!this.complaintData.description.trim() || this.complaintData.description.length < 50) {
        this.formState.description = false
        isValid = false
      }
      
      if (this.uploadedFiles.length === 0) {
        this.formState.documents = false
        isValid = false
      }
      
      if (!this.complaintData.declaration) {
        this.formState.declaration = false
        isValid = false
      }
      
      if (isValid) {
        this.submitComplaint()
      } else {
        this.$bvToast.toast('لطفا تمام فیلدهای الزامی را به درستی تکمیل کنید', {
          title: 'خطا در فرم',
          variant: 'danger',
          solid: true
        })
      }
    },
    
    async submitComplaint() {
      this.submitting = true
      
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 2000))
        
        // Generate tracking code
        this.newTrackingCode = this.generateTrackingCode()
        
        // Create new complaint object
        const newComplaint = {
          id: Date.now(),
          trackingCode: this.newTrackingCode,
          ...this.complaintData,
          status: 'pending',
          submittedDate: this.getCurrentDate(),
          lastUpdate: this.getCurrentDate(),
          documentsCount: this.uploadedFiles.length,
          preview: this.complaintData.description.substring(0, 150) + '...',
          documents: this.uploadedFiles.map(file => ({
            id: Date.now() + Math.random(),
            name: file.name,
            size: file.size,
            url: URL.createObjectURL(file),
            type: file.type
          }))
        }
        
        // Add to user complaints
        this.userComplaints.unshift(newComplaint)
        
        // Show success modal
        this.showSuccessModal = true
        
        // Reset form
        this.resetForm()
        
        // Switch to my complaints tab
        this.activeTab = 1
        
        this.$bvToast.toast('اعتراض شما با موفقیت ثبت شد', {
          title: 'ثبت موفق',
          variant: 'success',
          solid: true
        })
        
      } catch (error) {
        this.$bvToast.toast('خطا در ثبت اعتراض. لطفا مجددا تلاش کنید', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
      } finally {
        this.submitting = false
      }
    },
    
    resetForm() {
      this.complaintData = {
        decisionType: null,
        caseNumber: '',
        candidateName: '',
        candidateRegion: '',
        candidatePosition: '',
        subject: '',
        description: '',
        reasons: [],
        documents: [],
        urgency: 'normal',
        declaration: false
      }
      
      this.uploadedFiles = []
      
      // Reset validation states
      Object.keys(this.formState).forEach(key => {
        this.formState[key] = null
      })
    },
    
    // Complaint List Methods
    loadUserComplaints() {
      // In real app, fetch from API
      this.userComplaints = this.sampleComplaints
      this.totalComplaints = this.userComplaints.length
      this.totalPages = Math.ceil(this.totalComplaints / this.complaintsPerPage)
    },
    
    getComplaintStatusVariant(status) {
      const variants = {
        pending: 'warning',
        under_review: 'info',
        approved: 'success',
        rejected: 'danger',
        cancelled: 'secondary'
      }
      return variants[status] || 'secondary'
    },
    
    getComplaintStatusText(status) {
      const texts = {
        pending: 'در انتظار',
        under_review: 'در حال بررسی',
        approved: 'پذیرفته شده',
        rejected: 'رد شده',
        cancelled: 'لغو شده'
      }
      return texts[status] || status
    },
    
    viewComplaintDetails(complaint) {
      this.selectedComplaint = complaint
      this.showComplaintModal = true
    },
    
    editComplaint(complaint) {
      // Populate form with complaint data
      this.complaintData = { ...complaint }
      this.activeTab = 0
      
      this.$bvToast.toast('برای ویرایش اعتراض، تغییرات مورد نظر را اعمال و مجددا ثبت کنید', {
        title: 'ویرایش اعتراض',
        variant: 'info',
        solid: true
      })
    },
    
    cancelComplaint(complaint) {
      this.$bvModal.msgBoxConfirm('آیا از لغو این اعتراض اطمینان دارید؟', {
        title: 'لغو اعتراض',
        size: 'md',
        buttonSize: 'sm',
        okVariant: 'danger',
        okTitle: 'بله، لغو کن',
        cancelTitle: 'انصراف',
        centered: true
      }).then(value => {
        if (value) {
          complaint.status = 'cancelled'
          complaint.lastUpdate = this.getCurrentDate()
          
          this.$bvToast.toast('اعتراض با موفقیت لغو شد', {
            title: 'لغو موفق',
            variant: 'info',
            solid: true
          })
        }
      })
    },
    
    // Tracking Methods
    async trackComplaint() {
      if (!this.trackingCode || this.trackingCode.length !== 12) {
        this.trackingState = false
        return
      }
      
      this.trackingLoading = true
      
      try {
        // Simulate API call
        await new Promise(resolve => setTimeout(resolve, 1500))
        
        // Mock tracking result
        this.trackingResult = {
          trackingCode: this.trackingCode,
          subject: 'اعتراض به رد صلاحیت کاندیداتوری',
          status: 'under_review',
          submittedDate: '۱۴۰۲/۱۱/۱۰',
          lastUpdate: '۱۴۰۲/۱۱/۱۲',
          assignedTo: 'دکتر سید حسن موسوی',
          estimatedDate: '۱۴۰۲/۱۱/۲۰',
          timeline: [
            {
              title: 'ثبت اعتراض',
              date: '۱۴۰۲/۱۱/۱۰',
              description: 'اعتراض توسط کاربر ثبت شد',
              completed: true,
              active: false,
              icon: 'clipboard-check'
            },
            {
              title: 'بررسی اولیه',
              date: '۱۴۰۲/۱۱/۱۱',
              description: 'مدارک مورد بررسی قرار گرفت',
              completed: true,
              active: false,
              icon: 'search'
            },
            {
              title: 'ارجاع به کارشناس',
              date: '۱۴۰۲/۱۱/۱۲',
              description: 'به کارشناس مربوطه ارجاع داده شد',
              completed: true,
              active: false,
              icon: 'person-check'
            },
            {
              title: 'بررسی تخصصی',
              date: '۱۴۰۲/۱۱/۱۵',
              description: 'در حال بررسی تخصصی',
              completed: false,
              active: true,
              icon: 'file-earmark-text'
            },
            {
              title: 'صدور رأی',
              date: '۱۴۰۲/۱۱/۱۸',
              description: 'در انتظار صدور رأی',
              completed: false,
              active: false,
              icon: 'hammer'
            },
            {
              title: 'اعلام نتیجه',
              date: '۱۴۰۲/۱۱/۲۰',
              description: 'اعلام نتیجه نهایی',
              completed: false,
              active: false,
              icon: 'megaphone'
            }
          ],
          documents: [
            {
              id: 1,
              name: 'فرم اعتراض.pdf',
              type: 'فرم اصلی',
              date: '۱۴۰۲/۱۱/۱۰',
              url: '#'
            },
            {
              id: 2,
              name: 'پاسخ کارشناس.pdf',
              type: 'گزارش بررسی',
              date: '۱۴۰۲/۱۱/۱۲',
              url: '#'
            }
          ]
        }
        
        this.trackingState = true
        
      } catch (error) {
        this.$bvToast.toast('خطا در دریافت اطلاعات پیگیری', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        })
      } finally {
        this.trackingLoading = false
      }
    },
    
    submitFollowUp() {
      if (this.followUpMessage.trim()) {
        // In real app, send to API
        this.$bvToast.toast('پیام شما با موفقیت ارسال شد', {
          title: 'ارسال موفق',
          variant: 'success',
          solid: true
        })
        
        this.followUpMessage = ''
      }
    },
    
    // Utility Methods
    generateTrackingCode() {
      const date = new Date()
      const year = date.getFullYear() - 621 // Convert to Solar year
      const month = (date.getMonth() + 1).toString().padStart(2, '0')
      const day = date.getDate().toString().padStart(2, '0')
      const random = Math.floor(Math.random() * 10000).toString().padStart(4, '0')
      return `${year}/${month}/${day}${random}`
    },
    
    getCurrentDate() {
      return new Date().toLocaleDateString('fa-IR')
    },
    
    // Modal Methods
    printComplaint() {
      window.print()
      this.showSuccessModal = false
    },
    
    // Footer Methods
    downloadGuide() {
      // In real app, download PDF guide
      this.$bvToast.toast('راهنمای کاربری در حال دانلود...', {
        title: 'دانلود',
        variant: 'info',
        solid: true
      })
    },
    
    showFAQ() {
      this.$router.push('/faq')
    }
  }
}
</script>

<style scoped>
.user-complaint-page {
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Header */
.page-header {
  background: linear-gradient(135deg, #dc3545 0%, #fd7e14 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.complaint-icon {
  font-size: 2.5rem;
  color: #ffc107;
}

/* User Info Card */
.user-info-card {
  border-radius: 12px;
  border: 2px solid #dc3545;
  box-shadow: 0 4px 15px rgba(220, 53, 69, 0.1);
}

.user-avatar {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  object-fit: cover;
  border: 4px solid #dc3545;
}

.user-details {
  margin-top: 15px;
}

.detail-item {
  display: flex;
  align-items: center;
  margin-bottom: 8px;
  color: #555;
}

.detail-item strong {
  min-width: 100px;
  margin-left: 8px;
  margin-right: 5px;
}

/* Form Styles */
.document-upload-area {
  padding: 20px;
  border: 2px dashed #dee2e6;
  border-radius: 8px;
  background: #f8f9fa;
  transition: all 0.3s ease;
}

.document-upload-area:hover {
  border-color: #007bff;
  background: #e7f3ff;
}

.uploaded-files {
  background: white;
  border-radius: 6px;
  border: 1px solid #dee2e6;
  overflow: hidden;
}

.files-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  background: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
}

.files-list {
  max-height: 200px;
  overflow-y: auto;
}

.file-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  border-bottom: 1px solid #f0f0f0;
}

.file-item:last-child {
  border-bottom: none;
}

.file-info {
  display: flex;
  align-items: center;
  flex: 1;
}

.file-name {
  margin: 0 10px;
  flex: 1;
}

.file-size {
  color: #6c757d;
  min-width: 70px;
}

.declaration-card {
  background: #f8f9fa;
  border: 1px solid #dee2e6;
}

.submit-btn {
  min-width: 150px;
}

/* Complaints List */
.complaint-item {
  transition: transform 0.2s ease;
}

.complaint-item:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
}

.complaint-title {
  flex: 1;
  min-width: 300px;
}

.complaint-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
  margin-top: 5px;
}

.complaint-actions {
  flex-shrink: 0;
}

.complaint-preview {
  color: #666;
  line-height: 1.6;
}

.preview-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #f0f0f0;
}

/* Tracking */
.tracking-form {
  background: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  border: 1px solid #dee2e6;
}

.timeline {
  position: relative;
  padding: 20px 0;
}

.timeline-step {
  display: flex;
  margin-bottom: 30px;
  position: relative;
}

.timeline-step:last-child {
  margin-bottom: 0;
}

.timeline-step::before {
  content: '';
  position: absolute;
  right: 24px;
  top: 40px;
  bottom: -30px;
  width: 2px;
  background: #dee2e6;
}

.timeline-step:last-child::before {
  display: none;
}

.timeline-step.completed::before {
  background: #28a745;
}

.timeline-icon {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-left: 20px;
  z-index: 1;
  background: #6c757d;
  color: white;
}

.timeline-step.active .timeline-icon {
  background: #007bff;
  box-shadow: 0 0 0 5px rgba(0, 123, 255, 0.2);
}

.timeline-step.completed .timeline-icon {
  background: #28a745;
}

.timeline-content {
  flex: 1;
  background: white;
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #dee2e6;
}

.timeline-step.active .timeline-content {
  border-color: #007bff;
  box-shadow: 0 2px 5px rgba(0, 123, 255, 0.1);
}

.timeline-step.completed .timeline-content {
  border-color: #28a745;
}

.timeline-title {
  font-weight: bold;
  margin-bottom: 5px;
  color: #333;
}

.timeline-date {
  color: #6c757d;
  font-size: 0.9rem;
  margin-bottom: 5px;
}

.timeline-description {
  color: #666;
  font-size: 0.9rem;
  line-height: 1.5;
}

.timeline-officer {
  margin-top: 5px;
  color: #6c757d;
}

.status-summary {
  padding: 15px;
}

.status-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
  padding-bottom: 10px;
  border-bottom: 1px solid #dee2e6;
}

.status-details {
  display: grid;
  gap: 10px;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 5px 0;
  border-bottom: 1px solid #f0f0f0;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row strong {
  min-width: 150px;
}

/* Rules and Instructions */
.rules-section {
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.rules-section:last-child {
  border-bottom: none;
}

.section-title {
  color: #dc3545;
  display: flex;
  align-items: center;
  margin-bottom: 15px;
}

.rules-list {
  padding-right: 20px;
  color: #555;
}

.rules-list li {
  margin-bottom: 10px;
  line-height: 1.6;
}

.process-steps {
  display: grid;
  gap: 15px;
}

.step {
  display: flex;
  align-items: center;
  padding: 10px;
  background: #f8f9fa;
  border-radius: 6px;
  border-right: 4px solid #dc3545;
}

.step-number {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: #dc3545;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  margin-left: 15px;
  flex-shrink: 0;
}

.time-limits {
  display: grid;
  gap: 10px;
}

.time-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 4px;
}

.time-label {
  font-weight: 500;
}

.time-value {
  color: #dc3545;
  font-weight: bold;
}

.contact-info {
  display: grid;
  gap: 10px;
}

.contact-item {
  display: flex;
  align-items: center;
  color: #555;
}

.contact-item svg {
  margin-left: 10px;
  color: #dc3545;
}

/* Complaint Details Modal */
.complaint-details-modal {
  padding: 10px;
}

.tracking-code {
  font-family: 'Courier New', monospace;
  font-size: 1.2rem;
  color: #dc3545;
  background: #f8f9fa;
  padding: 5px 10px;
  border-radius: 4px;
  border: 1px dashed #dc3545;
}

.simple-timeline {
  position: relative;
  padding-right: 30px;
}

.simple-timeline::before {
  content: '';
  position: absolute;
  right: 0;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #dee2e6;
}

.timeline-item {
  position: relative;
  margin-bottom: 20px;
  padding-bottom: 20px;
}

.timeline-item:last-child {
  margin-bottom: 0;
  padding-bottom: 0;
}

.timeline-dot {
  position: absolute;
  right: -6px;
  top: 5px;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #dc3545;
  z-index: 1;
}

.timeline-content {
  background: white;
  padding: 15px;
  border-radius: 6px;
  border: 1px solid #dee2e6;
  margin-right: 20px;
}

/* Success Modal */
.success-modal {
  padding: 20px;
}

.tracking-code-display {
  padding: 20px;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-radius: 8px;
  border: 2px dashed #28a745;
}

.code {
  font-family: 'Courier New', monospace;
  font-size: 1.5rem;
  font-weight: bold;
  color: #28a745;
  margin: 10px 0;
  padding: 10px;
  background: white;
  border-radius: 4px;
  letter-spacing: 2px;
}

/* Footer */
.complaint-footer {
  background: #343a40;
  color: white;
  margin-top: 40px;
}

.footer-links a {
  color: rgba(255, 255, 255, 0.8);
  text-decoration: none;
  transition: color 0.3s ease;
}

.footer-links a:hover {
  color: white;
  text-decoration: underline;
}

/* Responsive Design */
@media (max-width: 768px) {
  .user-avatar {
    width: 80px;
    height: 80px;
  }
  
  .complaint-title {
    min-width: 100%;
  }
  
  .complaint-header {
    flex-direction: column;
    gap: 10px;
  }
  
  .complaint-actions {
    width: 100%;
    justify-content: flex-start;
  }
  
  .timeline-step {
    flex-direction: column;
  }
  
  .timeline-icon {
    margin: 0 auto 15px;
  }
  
  .timeline-step::before {
    right: 50%;
    transform: translateX(50%);
  }
  
  .timeline-content {
    margin-right: 0;
  }
  
  .detail-item {
    flex-direction: column;
    align-items: flex-start;
  }
  
  .detail-item strong {
    margin: 5px 0;
  }
}

@media print {
  .user-complaint-page {
    background: white !important;
  }
  
  .complaint-footer,
  .complaint-actions,
  .submit-btn {
    display: none !important;
  }
}
</style>