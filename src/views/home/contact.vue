<template>
  <div class="support-page">
    <!-- Header -->
    <b-container fluid class="support-header py-4">
      <b-row class="align-items-center">
        <b-col cols="12" md="8">
          <div class="d-flex align-items-center">
            <div class="support-icon mr-3">
              <b-icon icon="headset"></b-icon>
            </div>
            <div>
              <h2 class="mb-1">پشتیبانی و تماس</h2>
              <p class="text-muted mb-0">مرکز پاسخگویی به سؤالات و مشکلات انتخابات</p>
            </div>
          </div>
        </b-col>
        <b-col cols="12" md="4" class="text-left text-md-right">
          <div class="support-status">
            <b-badge variant="success" class="p-2">
              <b-icon icon="circle-fill" class="ml-1" font-scale="0.8"></b-icon>
              پشتیبانی آنلاین
            </b-badge>
            <div class="response-time mt-2">
              میانگین زمان پاسخ: <strong>۱۵ دقیقه</strong>
            </div>
          </div>
        </b-col>
      </b-row>
    </b-container>

    <!-- Quick Actions -->
    <b-container class="quick-actions mb-4">
      <b-row>
        <b-col md="3" class="mb-3">
          <b-card class="action-card text-center" @click="scrollToSection('tickets')">
            <div class="action-icon tickets">
              <b-icon icon="ticket-detailed"></b-icon>
            </div>
            <h5 class="mt-3">{{ ticketsTitle }}</h5>
            <p class="text-muted small">پیگیری و پاسخ‌گویی درخواست‌ها</p>
            <b-badge variant="info" pill>{{ ticketStats.total }}</b-badge>
          </b-card>
        </b-col>
        <b-col md="3" class="mb-3">
          <b-card class="action-card text-center" @click="scrollToSection('new-ticket')">
            <div class="action-icon new">
              <b-icon icon="plus-circle"></b-icon>
            </div>
            <h5 class="mt-3">درخواست جدید</h5>
            <p class="text-muted small">ثبت مشکل یا سؤال جدید</p>
          </b-card>
        </b-col>
        <b-col md="3" class="mb-3">
          <b-card class="action-card text-center" @click="scrollToSection('faq')">
            <div class="action-icon faq">
              <b-icon icon="question-circle"></b-icon>
            </div>
            <h5 class="mt-3">سؤالات متداول</h5>
            <p class="text-muted small">پاسخ به پرسش‌های رایج</p>
          </b-card>
        </b-col>
        <b-col md="3" class="mb-3">
          <b-card class="action-card text-center" @click="startLiveChat">
            <div class="action-icon chat">
              <b-icon icon="chat-dots"></b-icon>
            </div>
            <h5 class="mt-3">چت آنلاین</h5>
            <p class="text-muted small">گفتگوی مستقیم با پشتیبان</p>
            <b-badge variant="success" v-if="onlineAgents > 0">
              {{ onlineAgents }} پشتیبان آنلاین
            </b-badge>
          </b-card>
        </b-col>
      </b-row>
    </b-container>

    <!-- Main Content -->
    <b-container class="support-content">
      <b-row>
        <!-- Left Column: Tickets and FAQ -->
        <b-col lg="8" class="mb-4">
          <!-- User Tickets -->
          <b-card class="mb-4" id="tickets-section">
            <div class="d-flex justify-content-between align-items-center mb-4">
              <h5 class="mb-0">
                <b-icon icon="ticket-detailed" class="ml-2"></b-icon>
                {{ ticketsTitle }}
              </h5>
              <div class="ticket-filters">
                <b-button-group size="sm">
                  <b-button :variant="ticketFilter === 'all' ? 'primary' : 'outline-primary'"
                    @click="ticketFilter = 'all'">
                    همه
                  </b-button>
                  <b-button :variant="ticketFilter === 'open' ? 'primary' : 'outline-primary'"
                    @click="ticketFilter = 'open'">
                    باز
                  </b-button>
                  <b-button :variant="ticketFilter === 'closed' ? 'primary' : 'outline-primary'"
                    @click="ticketFilter = 'closed'">
                    بسته
                  </b-button>
                </b-button-group>
              </div>
            </div>

            <b-alert v-if="isSupportAgent" show variant="info" class="support-scope-alert">
              <b-icon icon="shield-check" class="ml-1"></b-icon>
              شما با نقش <strong>{{ supportScope.roleLabel }}</strong> در سطح
              <strong>{{ supportScope.levelLabel }}</strong> تیکت‌های مرتبط با محدوده
              <strong>{{ supportScope.regionName || 'ستاد' }}</strong> را مشاهده و پاسخ می‌دهید.
            </b-alert>

            <div v-if="loadingTickets" class="text-center py-4">
              <b-spinner variant="primary" class="ml-2"></b-spinner>
              در حال دریافت تیکت‌ها...
            </div>

            <div v-else-if="filteredTickets.length > 0">
              <div v-for="ticket in filteredTickets" :key="ticket.id" class="ticket-item"
                @click="viewTicketDetails(ticket)">
                <div class="ticket-header">
                  <div class="ticket-title">
                    <strong>{{ ticket.title }}</strong>
                    <b-badge :variant="getTicketStatusVariant(ticket.status)" class="mr-2">
                      {{ getTicketStatusText(ticket.status) }}
                    </b-badge>
                  </div>
                  <div class="ticket-meta">
                    <span class="ticket-date">{{ ticket.date }}</span>
                    <span class="ticket-id">#{{ ticket.id }}</span>
                  </div>
                </div>
                <div class="ticket-body">
                  <p class="ticket-preview">{{ ticket.preview }}</p>
                  <div class="ticket-info">
                    <span class="category">
                      <b-icon icon="tag" class="ml-1"></b-icon>
                      {{ getCategoryText(ticket.category) }}
                    </span>
                    <span class="last-update">
                      <b-icon icon="clock" class="ml-1"></b-icon>
                      آخرین پاسخ: {{ ticket.lastReply }}
                    </span>
                    <span class="category">
                      <b-icon icon="people" class="ml-1"></b-icon>
                      {{ ticket.targetRoleLabel || getRoleText(ticket.target_role) }} / {{ ticket.supportLevelLabel ||
                        getLevelText(ticket.support_level) }}
                    </span>
                    <span v-if="isSupportAgent" class="category">
                      <b-icon icon="geo-alt" class="ml-1"></b-icon>
                      {{ ticket.requester_region_name || ticket.regionName || '---' }}
                    </span>
                  </div>
                </div>
              </div>
            </div>

            <div v-else class="text-center py-5">
              <b-icon icon="inbox" font-scale="4" variant="secondary"></b-icon>
              <h5 class="mt-3">تیکتی یافت نشد</h5>
              <p class="text-muted">در محدوده فعلی تیکتی یافت نشد.</p>
              <b-button variant="primary" @click="scrollToSection('new-ticket')">
                <b-icon icon="plus" class="ml-1"></b-icon>
                ثبت درخواست جدید
              </b-button>
            </div>
          </b-card>

          <!-- FAQ Section -->
          <b-card class="mb-4" id="faq-section">
            <h5 class="mb-4">
              <b-icon icon="question-circle" class="ml-2"></b-icon>
              سؤالات متداول (FAQ)
            </h5>

            <b-form-group class="mb-4">
              <b-input-group>
                <template #prepend>
                  <b-input-group-text>
                    <b-icon icon="search"></b-icon>
                  </b-input-group-text>
                </template>
                <b-form-input v-model="faqSearch" placeholder="جستجو در سؤالات متداول..."></b-form-input>
              </b-input-group>
            </b-form-group>

            <b-accordion v-model="faqOpenItems" v-if="false">
              <div v-for="(category, index) in filteredFaq" :key="category.id">
                <b-accordion-item :title="category.title" :id="`faq-category-${index}`">
                  <div class="faq-category-items">
                    <div v-for="(item, itemIndex) in category.items" :key="item.id" class="faq-item"
                      @click="toggleFaqItem(item.id)">
                      <div class="faq-question">
                        <b-icon :icon="openFaqItems.includes(item.id) ? 'chevron-down' : 'chevron-left'"
                          class="ml-2"></b-icon>
                        {{ item.question }}
                      </div>
                      <b-collapse :id="`faq-answer-${item.id}`" :visible="isFaqOpen(item.id)">
                        <div class="faq-answer">
                          <p>{{ item.answer }}</p>
                          <div v-if="item.relatedLinks && item.relatedLinks.length > 0" class="related-links">
                            <strong>لینک‌های مرتبط:</strong>
                            <ul>
                              <li v-for="(link, linkIndex) in item.relatedLinks" :key="linkIndex">
                                <a :href="link.url" target="_blank">{{ link.text }}</a>
                              </li>
                            </ul>
                          </div>
                        </div>
                      </b-collapse>
                    </div>
                  </div>
                </b-accordion-item>
              </div>
            </b-accordion>

            <div class="text-center mt-4">
              <b-button variant="outline-primary" @click="loadMoreFaq">
                <b-spinner small v-if="loadingFaq" class="ml-1"></b-spinner>
                مشاهده سؤالات بیشتر
              </b-button>
            </div>
          </b-card>
        </b-col>

        <!-- Right Column: New Ticket and Contact Info -->
        <b-col lg="4">
          <!-- New Ticket Form -->
          <b-card class="mb-4" id="new-ticket-section">
            <h5 class="mb-4">
              <b-icon icon="plus-circle" class="ml-2"></b-icon>
              ثبت درخواست جدید
            </h5>

            <b-form @submit.prevent="submitNewTicket">
              <b-form-group label="موضوع درخواست" label-for="ticket-subject">
                <b-form-input id="ticket-subject" v-model="newTicket.subject" :state="ticketValidation.subject"
                  placeholder="موضوع درخواست خود را وارد کنید" required></b-form-input>
                <b-form-invalid-feedback>
                  لطفاً موضوع درخواست را وارد کنید
                </b-form-invalid-feedback>
              </b-form-group>

              <b-form-group label="دسته‌بندی" label-for="ticket-category">
                <b-form-select id="ticket-category" v-model="newTicket.category" :options="ticketCategories" required
                  :state="ticketValidation.category"></b-form-select>
                <b-form-invalid-feedback>
                  لطفاً دسته‌بندی را انتخاب کنید
                </b-form-invalid-feedback>
              </b-form-group>
              <b-form-group label="واحد پاسخگو" label-for="ticket-target-role">
                <b-form-select id="ticket-target-role" v-model="newTicket.targetRole" :options="targetRoleOptions"
                  required></b-form-select>
                <small class="form-text text-muted">
                  تیکت بر اساس منطقه/استان شما فقط برای واحد انتخاب‌شده در همان سطح قابل مشاهده خواهد بود.
                </small>
              </b-form-group>
              <b-form-group label="اولویت" label-for="ticket-priority">
                <b-form-select id="ticket-priority" v-model="newTicket.priority" :options="priorityOptions" required
                  :state="ticketValidation.priority"></b-form-select>
                <small class="form-text text-muted">
                  <b-icon :icon="getPriorityIcon(newTicket.priority)" class="ml-1"></b-icon>
                  {{ getPriorityDescription(newTicket.priority) }}
                </small>
              </b-form-group>

              <b-form-group label="شرح کامل مشکل" label-for="ticket-description">
                <b-form-textarea id="ticket-description" v-model="newTicket.description" rows="5"
                  :state="ticketValidation.description" placeholder="شرح کامل مشکل یا سؤال خود را وارد کنید..."
                  required></b-form-textarea>
                <b-form-invalid-feedback>
                  لطفاً شرح درخواست را وارد کنید
                </b-form-invalid-feedback>
                <small class="form-text text-muted">
                  {{ newTicket.description.length }}/1000 کاراکتر
                </small>
              </b-form-group>

              <!-- File Upload -->
              <b-form-group label="ضمیمه (اختیاری)" label-for="ticket-attachments">
                <b-form-file id="ticket-attachments" v-model="newTicket.attachments" multiple
                  accept="image/*,.pdf,.doc,.docx,.txt" :file-name-formatter="formatFileNames"
                  placeholder="فایل‌ها را انتخاب کنید یا اینجا بکشید"
                  drop-placeholder="فایل‌ها را اینجا رها کنید"></b-form-file>
                <small class="form-text text-muted">
                  حداکثر ۵ فایل، هر کدام تا ۵ مگابایت (تصویر، PDF، Word، متن)
                </small>

                <!-- Uploaded Files Preview -->
                <div v-if="newTicket.attachments && newTicket.attachments.length > 0" class="mt-3">
                  <div class="file-preview" v-for="(file, index) in newTicket.attachments" :key="index">
                    <div class="file-info">
                      <b-icon :icon="getFileIcon(file.name)" class="ml-2"></b-icon>
                      <span class="file-name">{{ file.name }}</span>
                      <span class="file-size">{{ formatFileSize(file.size) }}</span>
                    </div>
                    <b-button size="sm" variant="outline-danger" @click="removeAttachment(index)">
                      <b-icon icon="x"></b-icon>
                    </b-button>
                  </div>
                </div>
              </b-form-group>

              <div class="text-center mt-4">
                <b-button type="submit" variant="primary" :disabled="submittingTicket" block>
                  <b-spinner small v-if="submittingTicket" class="ml-1"></b-spinner>
                  <span v-else>
                    <b-icon icon="send" class="ml-1"></b-icon>
                    ارسال درخواست
                  </span>
                </b-button>
              </div>
            </b-form>
          </b-card>

          <!-- Contact Information -->
          <b-card class="contact-card">
            <h5 class="mb-4">
              <b-icon icon="telephone" class="ml-2"></b-icon>
              راه‌های ارتباطی
            </h5>

            <div class="contact-methods">
              <div class="contact-item phone">
                <div class="contact-icon">
                  <b-icon icon="telephone-fill"></b-icon>
                </div>
                <div class="contact-details">
                  <div class="contact-title">تلفن پشتیبانی</div>
                  <div class="contact-value">۰۲۱-۸۸۵۶۱۲۳۴</div>
                  <div class="contact-hours">
                    <small class="text-muted">شنبه تا پنجشنبه، ۸ صبح تا ۱۶ عصر</small>
                  </div>
                </div>
              </div>

              <!-- <div class="contact-item email">
                <div class="contact-icon">
                  <b-icon icon="envelope-fill"></b-icon>
                </div>
                <div class="contact-details">
                  <div class="contact-title">ایمیل</div>
                  <div class="contact-value">support@farhangian-election.ir</div>
                  <div class="contact-hours">
                    <small class="text-muted">پاسخ‌دهی در ۲۴ ساعت</small>
                  </div>
                </div>
              </div> -->

              <div class="contact-item address">
                <div class="contact-icon">
                  <b-icon icon="geo-alt-fill"></b-icon>
                </div>
                <div class="contact-details">
                  <div class="contact-title">آدرس دفتر</div>
                  <div class="contact-value">تهران، خیابان ولیعصر، پلاک ۱۰۰۰</div>
                  <div class="contact-hours">
                    <small class="text-muted">مراجعه با هماهنگی قبلی</small>
                  </div>
                </div>
              </div>
            </div>

            <hr class="my-4">

            <div class="social-links">
              <h6 class="mb-3">شبکه‌های اجتماعی</h6>
              <div class="social-icons">
                <b-button variant="outline-primary" class="social-btn" @click="openSocial('telegram')">
                  <b-icon icon="telegram"></b-icon>
                </b-button>
                <b-button variant="outline-info" class="social-btn" @click="openSocial('instagram')">
                  <b-icon icon="instagram"></b-icon>
                </b-button>
                <b-button variant="outline-secondary" class="social-btn" @click="openSocial('twitter')">
                  <b-icon icon="twitter"></b-icon>
                </b-button>
                <b-button variant="outline-success" class="social-btn" @click="openSocial('whatsapp')">
                  <b-icon icon="whatsapp"></b-icon>
                </b-button>
              </div>
            </div>
          </b-card>
        </b-col>
      </b-row>
    </b-container>

    <!-- Live Chat Modal -->
    <b-modal v-model="showLiveChat" title="چت آنلاین پشتیبانی" hide-footer size="lg" centered
      @hide="handleLiveChatModalHide">
      <div class="live-chat-container">
        <!-- Chat Header -->
        <div class="chat-header">
          <div class="agent-info">
            <div class="agent-avatar">
              <b-icon icon="person-circle"></b-icon>
            </div>
            <div class="agent-details">
              <div class="agent-name">
                <strong>{{ chatHeaderTitle }}</strong>
                <b-badge :variant="currentChatSession?.status === 'closed' ? 'secondary' : 'success'" class="mr-2">
                  {{ getChatStatusText(currentChatSession?.status) }}
                </b-badge>
              </div>
              <div class="agent-status">در حال تایپ...</div>
            </div>
          </div>
          <div class="chat-actions">
            <b-button size="sm" variant="outline-secondary" @click="downloadChat">
              <b-icon icon="download"></b-icon>
            </b-button>
            <b-button size="sm" variant="outline-danger" @click="endChat">
              <b-icon icon="x"></b-icon>
            </b-button>
          </div>
        </div>

        <div v-if="isSupportAgent" class="chat-session-list mb-3">
          <div class="d-flex justify-content-between align-items-center mb-2">
            <strong>گفتگوهای زنده</strong>
            <b-button size="sm" variant="outline-primary" @click="loadLiveChatSessions" :disabled="loadingChatSessions">
              <b-spinner small v-if="loadingChatSessions" class="ml-1"></b-spinner>
              بروزرسانی
            </b-button>
          </div>
          <div v-if="liveChatSessions.length === 0" class="text-muted small text-center py-2">گفتگوی در انتظار پاسخ وجود
            ندارد.</div>
          <div v-for="session in liveChatSessions" :key="session.id" class="chat-session-item"
            :class="{ active: currentChatSession && currentChatSession.id === session.id }"
            @click="selectLiveChatSession(session)">
            <div class="d-flex justify-content-between">
              <strong>{{ session.userName || session.userNationalId }}</strong>
              <b-badge
                :variant="session.status === 'waiting' ? 'warning' : session.status === 'active' ? 'success' : 'secondary'">
                {{ getChatStatusText(session.status) }}
              </b-badge>
            </div>
            <div class="small text-muted">{{ session.subject || 'گفتگوی پشتیبانی' }}</div>
            <div class="small text-truncate">{{ session.lastMessage }}</div>
          </div>
        </div>

        <!-- Chat Messages -->
        <div class="chat-messages" ref="chatMessages">
          <div v-if="!currentChatSession" class="text-center text-muted py-5">
            برای شروع پاسخ‌گویی، یک گفتگو را انتخاب کنید.
          </div>
          <div v-for="message in chatMessages" :key="message.id" :class="`message ${message.sender}`">
            <div class="message-content">
              <div class="message-author" v-if="message.senderName">{{ message.senderName }}</div>
              <div class="message-text">{{ message.text }}</div>
              <div class="message-time">{{ message.time }}</div>
            </div>
          </div>
        </div>

        <!-- Chat Input -->
        <div class="chat-input">
          <b-input-group>
            <b-form-input v-model="chatInput" placeholder="پیام خود را بنویسید..." @keyup.enter="sendChatMessage"
              :disabled="!chatActive || !currentChatSession || currentChatSession.status === 'closed'"></b-form-input>
            <template #append>
              <b-button variant="primary" @click="sendChatMessage"
                :disabled="!chatInput.trim() || !chatActive || !currentChatSession || currentChatSession.status === 'closed'">
                <b-icon icon="send"></b-icon>
              </b-button>
            </template>
          </b-input-group>

          <div class="chat-options mt-2">
            <b-button size="sm" variant="outline-secondary"
              @click="sendQuickResponse(isSupportAgent ? 'سلام، من پشتیبان آنلاین هستم. لطفاً مشکل را توضیح دهید.' : 'در حال انتظار برای پشتیبان...')">
              <b-icon icon="clock"></b-icon>
            </b-button>
            <b-button size="sm" variant="outline-secondary" @click="sendQuickResponse('مشکل فنی دارم')">
              <b-icon icon="bug"></b-icon>
            </b-button>
            <b-button size="sm" variant="outline-secondary" @click="sendQuickResponse('سؤال درباره انتخابات دارم')">
              <b-icon icon="question-circle"></b-icon>
            </b-button>
            <b-button size="sm" variant="outline-secondary" @click="attachFileToChat">
              <b-icon icon="paperclip"></b-icon>
            </b-button>
          </div>
        </div>
      </div>
    </b-modal>

    <!-- Ticket Details Modal -->
    <b-modal v-model="showTicketModal" :title="`تیکت #${selectedTicket?.id}`" size="lg" hide-footer centered scrollable>
      <div v-if="selectedTicket" class="ticket-details">
        <!-- Ticket Header -->
        <div class="ticket-detail-header mb-4">
          <h5>{{ selectedTicket.title }}</h5>
          <div class="ticket-meta">
            <b-badge :variant="getTicketStatusVariant(selectedTicket.status)" class="mr-2">
              {{ getTicketStatusText(selectedTicket.status) }}
            </b-badge>
            <span class="text-muted ml-3">تاریخ ایجاد: {{ selectedTicket.date }}</span>
            <span class="text-muted ml-3">اولویت: {{ getPriorityText(selectedTicket.priority) }}</span>
            <span class="text-muted ml-3">واحد پاسخگو: {{ selectedTicket.targetRoleLabel ||
              getRoleText(selectedTicket.target_role) }}</span>
            <span class="text-muted ml-3">سطح: {{ selectedTicket.supportLevelLabel ||
              getLevelText(selectedTicket.support_level) }}</span>
          </div>
        </div>

        <!-- Ticket Conversation -->
        <div class="ticket-conversation">
          <div v-for="message in selectedTicket.conversation" :key="message.id" class="conversation-message"
            :class="`message-${message.sender}`">
            <div class="message-header">
              <div class="sender-info">
                <b-icon :icon="message.sender === 'user' ? 'person' : 'headset'" class="ml-2"></b-icon>
                <strong>{{ getMessageSenderText(message) }}</strong>
              </div>
              <div class="message-time">{{ message.time }}</div>
            </div>
            <div class="message-body">
              <p>{{ message.text }}</p>
              <div v-if="message.attachments && message.attachments.length > 0" class="message-attachments">
                <div v-for="(attachment, index) in message.attachments" :key="index" class="attachment-item">
                  <b-icon icon="paperclip" class="ml-2"></b-icon>
                  <a :href="attachment.url" target="_blank">{{ attachment.name }}</a>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Reply Section -->
        <div class="ticket-reply mt-4">
          <h6 class="mb-3">پاسخ جدید</h6>
          <b-form @submit.prevent="submitTicketReply">
            <b-form-textarea v-model="ticketReply" rows="3" placeholder="پاسخ خود را بنویسید..."
              :disabled="selectedTicket.status === 'closed' || !selectedTicket.canReply" required></b-form-textarea>

            <div class="d-flex justify-content-between align-items-center mt-3">
              <div>
                <b-form-checkbox v-model="closeTicketAfterReply" :disabled="selectedTicket.status === 'closed'">
                  بستن تیکت پس از پاسخ
                </b-form-checkbox>
              </div>
              <div>
                <b-button type="submit" variant="primary"
                  :disabled="!ticketReply.trim() || selectedTicket.status === 'closed' || !selectedTicket.canReply">
                  ارسال پاسخ
                </b-button>
                <b-button variant="outline-secondary" class="mr-3" @click="closeTicket"
                  v-if="selectedTicket.status !== 'closed' || !selectedTicket.canReply">
                  بستن تیکت
                </b-button>
              </div>
            </div>
          </b-form>
        </div>
      </div>
    </b-modal>

    <!-- Support Stats -->
    <b-container fluid class="support-stats py-4">
      <b-row>
        <b-col md="3" class="text-center">
          <div class="stat-item">
            <div class="stat-number">{{ supportStats.totalTickets }}</div>
            <div class="stat-label">کل درخواست‌ها</div>
          </div>
        </b-col>
        <b-col md="3" class="text-center">
          <div class="stat-item">
            <div class="stat-number">{{ supportStats.avgResponseTime }}</div>
            <div class="stat-label">میانگین زمان پاسخ (دقیقه)</div>
          </div>
        </b-col>
        <b-col md="3" class="text-center">
          <div class="stat-item">
            <div class="stat-number">{{ supportStats.satisfactionRate }}%</div>
            <div class="stat-label">رضایت کاربران</div>
          </div>
        </b-col>
        <b-col md="3" class="text-center">
          <div class="stat-item">
            <div class="stat-number">{{ supportStats.onlineAgents }}</div>
            <div class="stat-label">پشتیبان آنلاین</div>
          </div>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
import { mapGetters, mapMutations, mapActions } from "vuex";
export default {
  name: "SupportPage",
  data() {
    return {
      // Support Stats
      supportStats: {
        totalTickets: 1247,
        avgResponseTime: 15,
        satisfactionRate: 94,
        onlineAgents: 3
      },
      loadingTickets: false,
      supportScope: {},
      // Ticket Statistics
      ticketStats: {
        total: 5,
        open: 2,
        closed: 3,
        pending: 0
      },

      // Tickets Data
      tickets: [
      ],

      // Ticket Filter
      ticketFilter: 'all',

      // New Ticket Form
      newTicket: {
        subject: '',
        category: null,
        priority: 'medium',
        description: '',
        attachments: [],
        targetRole: 'EXECUTIVE'
      },
      ticketValidation: {
        subject: null,
        category: null,
        priority: null,
        description: null
      },
      submittingTicket: false,

      // Ticket Categories
      ticketCategories: [
        { value: null, text: 'انتخاب دسته‌بندی', disabled: true },
        { value: 'authentication', text: 'احراز هویت و ورود' },
        { value: 'voting', text: 'رأی‌گیری و انتخابات' },
        { value: 'candidates', text: 'کاندیداها و اطلاعات' },
        { value: 'technical', text: 'مشکلات فنی' },
        { value: 'account', text: 'حساب کاربری' },
        { value: 'other', text: 'سایر موارد' }
      ],
      targetRoleOptions: [
        { value: 'EXECUTIVE', text: 'اجرایی' },
        { value: 'SUPERVISOR', text: 'نظارت' },
        { value: 'ADMIN', text: 'ستاد/ادمین' }
      ],
      // Priority Options
      priorityOptions: [
        { value: 'low', text: 'کم' },
        { value: 'medium', text: 'متوسط' },
        { value: 'high', text: 'بالا' },
        { value: 'urgent', text: 'فوری' }
      ],

      // FAQ Data
      faqData: [
        {
          id: 1,
          title: 'احراز هویت و ورود',
          items: [
            {
              id: 101,
              question: 'چگونه در سامانه ثبت‌نام کنم؟',
              answer: 'برای ثبت‌نام در سامانه انتخابات، روی دکمه "ثبت‌نام" در صفحه اصلی کلیک کرده و اطلاعات هویتی خود شامل کد ملی، شماره همراه و ایمیل را وارد کنید. پس از تأیید اطلاعات، کد فعال‌سازی برای شما ارسال می‌شود.',
              relatedLinks: [
                { text: 'راهنمای تصویری ثبت‌نام', url: '#' },
                { text: 'ویدیوی آموزشی', url: '#' }
              ]
            },
            {
              id: 102,
              question: 'کد تأیید برای من ارسال نمی‌شود، چه کنم؟',
              answer: 'ابتدا از صحت شماره همراه خود اطمینان حاصل کنید. اگر شماره صحیح است، صندوق پیامک خود را بررسی کنید. در صورت عدم دریافت پس از ۵ دقیقه، دکمه "ارسال مجدد کد" را بزنید. اگر مشکل ادامه داشت، با پشتیبانی تماس بگیرید.',
              relatedLinks: []
            },
            {
              id: 103,
              question: 'رمز عبور خود را فراموش کرده‌ام',
              answer: 'در صفحه ورود، روی گزینه "فراموشی رمز عبور" کلیک کنید. کد ملی و شماره همراه خود را وارد کرده و دستورالعمل بازیابی رمز که برای شما ارسال می‌شود را دنبال کنید.',
              relatedLinks: [
                { text: 'بازیابی رمز عبور', url: '#' }
              ]
            }
          ]
        },
        {
          id: 2,
          title: 'رأی‌گیری الکترونیکی',
          items: [
            {
              id: 201,
              question: 'آیا رأی دادن امن است؟',
              answer: 'بله، سامانه رأی‌گیری با بالاترین استانداردهای امنیتی طراحی شده است. اطلاعات شما رمزنگاری شده و رأی شما به صورت کاملاً محرمانه ثبت می‌شود.',
              relatedLinks: [
                { text: 'سیاست‌های امنیتی', url: '#' }
              ]
            },
            {
              id: 202,
              question: 'آیا پس از ثبت رأی می‌توانم آن را تغییر دهم؟',
              answer: 'خیر، پس از ثبت نهایی رأی، امکان تغییر آن وجود ندارد. لطفاً قبل از تأیید نهایی، انتخاب خود را با دقت بررسی کنید.',
              relatedLinks: []
            },
            {
              id: 203,
              question: 'چگونه می‌توانم از ثبت رأی خود اطمینان حاصل کنم؟',
              answer: 'پس از ثبت موفقیت‌آمیز رأی، شماره پیگیری منحصر به فردی دریافت خواهید کرد. همچنین می‌توانید از طریق بخش "پیگیری رأی" با وارد کردن کد ملی، وضعیت رأی خود را بررسی کنید.',
              relatedLinks: [
                { text: 'پیگیری رأی', url: '#' }
              ]
            }
          ]
        },
        {
          id: 3,
          title: 'کاندیداها و انتخابات',
          items: [
            {
              id: 301,
              question: 'شرایط کاندیداتوری چیست؟',
              answer: 'شرایط کاندیداتوری شامل التزام به قانون اساسی، داشتن حسن شهرت، برخورداری از سلامت کامل، عدم اعتیاد، عدم محکومیت مالی و تعهد به شفافیت می‌باشد. جزئیات کامل در صفحه شرایط کاندیداتوری موجود است.',
              relatedLinks: [
                { text: 'شرایط کامل کاندیداتوری', url: '#' }
              ]
            },
            {
              id: 302,
              question: 'آخرین مهلت ثبت‌نام کاندیداتوری چه زمانی است؟',
              answer: 'مهلت ثبت‌نام کاندیداتوری تا تاریخ ۱۴۰۲/۱۱/۲۰ می‌باشد. توصیه می‌شود حداقل ۴۸ ساعت قبل از مهلت نهایی، ثبت‌نام خود را تکمیل کنید.',
              relatedLinks: [
                { text: 'تقویم انتخابات', url: '#' }
              ]
            }
          ]
        }
      ],

      // FAQ State
      faqSearch: '',
      faqOpenItems: [],
      openFaqItems: [],
      loadingFaq: false,

      // Live Chat
      showLiveChat: false,
      chatActive: false,
      chatInput: '',
      chatMessages: [],
      liveChatSessions: [],
      currentChatSession: null,
      liveChatPollingId: null,
      loadingChatSessions: false,
      onlineAgents: 0,

      // Ticket Modal
      showTicketModal: false,
      selectedTicket: null,
      ticketReply: '',
      closeTicketAfterReply: false
    };
  },
  computed: {
    ...mapGetters(["currentUser"]),

    userRole() {
      const roles = this.currentUser?.roles;
      return Array.isArray(roles) ? roles[0] : roles;
    },

    isSupportAgent() {
      return ['ADMIN', 'SUPERVISOR', 'EXECUTIVE'].includes(this.userRole);
    },

    ticketsTitle() {
      return this.isSupportAgent ? 'تیکت‌های قابل پاسخ' : 'تیکت‌های من';
    },
    chatHeaderTitle() {
      if (!this.currentChatSession) return this.isSupportAgent ? 'میز پاسخ‌گویی زنده' : 'پشتیبان آنلاین';
      return this.isSupportAgent
        ? (this.currentChatSession.userName || this.currentChatSession.userNationalId || 'کاربر')
        : (this.currentChatSession.assignedAgentName || 'پشتیبان آنلاین');
    },

    chatHeaderSubtitle() {
      if (!this.currentChatSession) return this.isSupportAgent ? 'گفتگوی در انتظار پاسخ را انتخاب کنید' : 'در انتظار اتصال به پشتیبان';
      if (this.currentChatSession.status === 'waiting') return 'در صف پاسخ‌گویی زنده';
      if (this.currentChatSession.status === 'active') return 'گفتگو فعال است و پیام‌ها به صورت خودکار بروزرسانی می‌شوند';
      return 'گفتگو بسته شده است';
    },
    filteredTickets() {
      if (this.ticketFilter === 'all') {
        return this.tickets;
      }
      return this.tickets.filter(ticket => ticket.status === this.ticketFilter);
    },

    filteredFaq() {
      if (!this.faqSearch) {
        return this.faqData;
      }

      const query = this.faqSearch.toLowerCase();
      return this.faqData
        .map(category => {
          const filteredItems = category.items.filter(item =>
            item.question.toLowerCase().includes(query) ||
            item.answer.toLowerCase().includes(query)
          );

          if (filteredItems.length > 0) {
            return {
              ...category,
              items: filteredItems
            };
          }
          return null;
        })
        .filter(category => category !== null);
    }
  },
  mounted() {
    this.loadSupportTickets();
  }, beforeDestroy() {
    this.stopLiveChatPolling();
  },
  methods: {
    ...mapActions({
      getSupportTickets: "getSupportTickets",
      saveSupportTicket: "saveSupportTicket",
      replySupportTicket: "replySupportTicket",
      startLiveChatSession: "startLiveChat",
      getLiveChatSessions: "getLiveChatSessions",
      getLiveChatMessages: "getLiveChatMessages",
      sendLiveChatMessage: "sendLiveChatMessage",
      closeLiveChatSession: "closeLiveChat"
    }),
    async loadSupportTickets() {
      this.loadingTickets = true;
      try {
        const response = await this.getSupportTickets()
        if (response && response.status) {
          this.tickets = (response.data || []).map(this.normalizeTicket);
          this.ticketStats = response.stats || this.calculateTicketStats(this.tickets);
          this.supportScope = response.scope || {};
          this.supportStats.totalTickets = this.ticketStats.total;
        }
      } catch (error) {
        console.error('Load support tickets error:', error);
      } finally {
        this.loadingTickets = false;
      }
    },

    normalizeTicket(ticket) {
      const conversation = ticket.conversation || [];
      const firstMessage = conversation[0]?.text || conversation[0]?.message || '';
      return {
        ...ticket,
        id: ticket.id,
        code: ticket.ticket_code || ticket.code || ticket.id,
        title: ticket.subject || ticket.title,
        target_role: ticket.target_role || ticket.targetRole,
        support_level: ticket.support_level || ticket.supportLevel,
        preview: ticket.preview || (firstMessage ? firstMessage.substring(0, 100) + (firstMessage.length > 100 ? '...' : '') : ''),
        date: ticket.date || ticket.created_at,
        lastReply: ticket.lastReply || ticket.updated_at,
        canReply: ticket.canReply !== false,
        conversation: conversation.map(message => ({
          ...message,
          text: message.text || message.message,
          sender: message.sender || (message.sender_role === 'USER' ? 'user' : 'support'),
          attachments: message.attachments || []
        }))
      };
    },

    calculateTicketStats(tickets) {
      return tickets.reduce((stats, ticket) => {
        stats.total++;
        if (stats[ticket.status] !== undefined) stats[ticket.status]++;
        return stats;
      }, { total: 0, open: 0, closed: 0, pending: 0 });
    },
    isFaqOpen(itemId) {
      return this.openFaqItems.includes(itemId);
    },

    // Navigation
    scrollToSection(sectionId) {
      const element = document.getElementById(sectionId + '-section');
      if (element) {
        element.scrollIntoView({ behavior: 'smooth' });
      }
    },

    // Ticket Methods
    getTicketStatusVariant(status) {
      const variants = {
        open: 'warning',
        closed: 'success',
        pending: 'info'
      };
      return variants[status] || 'secondary';
    },

    getTicketStatusText(status) {
      const texts = {
        open: 'باز',
        closed: 'بسته',
        pending: 'در انتظار'
      };
      return texts[status] || status;
    },

    getCategoryText(category) {
      const categoryMap = {
        authentication: 'احراز هویت',
        voting: 'رأی‌گیری',
        candidates: 'کاندیداها',
        technical: 'فنی',
        account: 'حساب کاربری',
        other: 'سایر'
      };
      return categoryMap[category] || category;
    },
    getRoleText(role) {
      const roleMap = {
        ADMIN: 'ستاد/ادمین',
        SUPERVISOR: 'نظارت',
        EXECUTIVE: 'اجرایی',
        USER: 'کاربر'
      };
      return roleMap[role] || role || '---';
    },

    getLevelText(level) {
      const levelMap = {
        headquarters: 'ستاد',
        province: 'استان',
        region: 'منطقه'
      };
      return levelMap[level] || level || '---';
    },

    getMessageSenderText(message) {
      if (message.senderLabel) return message.senderLabel;
      if (message.sender === 'user') return 'کاربر/شما';
      return 'پشتیبان';
    },

    viewTicketDetails(ticket) {
      this.selectedTicket = ticket;
      this.ticketReply = '';
      this.closeTicketAfterReply = false;
      this.showTicketModal = true;
    },

    // New Ticket Submission
    async submitNewTicket() {
      // Validate form
      let valid = true;

      if (!this.newTicket.subject.trim()) {
        this.ticketValidation.subject = false;
        valid = false;
      } else {
        this.ticketValidation.subject = true;
      }

      if (!this.newTicket.category) {
        this.ticketValidation.category = false;
        valid = false;
      } else {
        this.ticketValidation.category = true;
      }

      if (!this.newTicket.priority) {
        this.ticketValidation.priority = false;
        valid = false;
      } else {
        this.ticketValidation.priority = true;
      }

      if (!this.newTicket.description.trim() || this.newTicket.description.length < 10) {
        this.ticketValidation.description = false;
        valid = false;
      } else {
        this.ticketValidation.description = true;
      }

      if (!valid) return;

      this.submittingTicket = true;

      try {
        const response = await this.saveSupportTicket({
          subject: this.newTicket.subject,
          category: this.newTicket.category,
          priority: this.newTicket.priority,
          description: this.newTicket.description,
          targetRole: this.newTicket.targetRole
        });

        if (!response || !response.status) {
          throw new Error(response?.message || 'خطا در ثبت درخواست');
        }

        // Reset form
        this.resetNewTicketForm();
        await this.loadSupportTickets();
        // Show success message
        this.$bvToast.toast('تیکت شما با موفقیت ثبت شد', {
          title: 'ثبت درخواست',
          variant: 'success',
          solid: true
        });

        // Scroll to tickets section
        this.scrollToSection('tickets');

      } catch (error) {
        console.error('Ticket submission error:', error);
        this.$bvToast.toast(error.message || 'خطا در ثبت درخواست', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      } finally {
        this.submittingTicket = false;
      }
    },

    resetNewTicketForm() {
      this.newTicket = {
        subject: '',
        category: null,
        priority: 'medium',
        description: '',
        attachments: [],
        targetRole: 'EXECUTIVE'
      };
      this.ticketValidation = {
        subject: null,
        category: null,
        priority: null,
        description: null
      };
    },

    // File Methods
    formatFileNames(files) {
      if (files.length === 1) {
        return files[0].name;
      } else {
        return `${files.length} فایل انتخاب شده`;
      }
    },

    getFileIcon(filename) {
      const extension = filename.split('.').pop().toLowerCase();
      const iconMap = {
        pdf: 'file-earmark-pdf',
        doc: 'file-earmark-word',
        docx: 'file-earmark-word',
        txt: 'file-earmark-text',
        jpg: 'file-earmark-image',
        jpeg: 'file-earmark-image',
        png: 'file-earmark-image',
        gif: 'file-earmark-image'
      };
      return iconMap[extension] || 'file-earmark';
    },

    formatFileSize(bytes) {
      if (bytes === 0) return '0 بایت';
      const k = 1024;
      const sizes = ['بایت', 'کیلوبایت', 'مگابایت'];
      const i = Math.floor(Math.log(bytes) / Math.log(k));
      return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
    },

    removeAttachment(index) {
      this.newTicket.attachments.splice(index, 1);
    },

    // Priority Methods
    getPriorityIcon(priority) {
      const icons = {
        low: 'arrow-down',
        medium: 'dash',
        high: 'arrow-up',
        urgent: 'exclamation-triangle'
      };
      return icons[priority] || 'dash';
    },

    getPriorityDescription(priority) {
      const descriptions = {
        low: 'پاسخ‌دهی در ۴۸ ساعت',
        medium: 'پاسخ‌دهی در ۲۴ ساعت',
        high: 'پاسخ‌دهی در ۱۲ ساعت',
        urgent: 'پاسخ‌دهی در ۲ ساعت'
      };
      return descriptions[priority] || '';
    },

    getPriorityText(priority) {
      const texts = {
        low: 'کم',
        medium: 'متوسط',
        high: 'بالا',
        urgent: 'فوری'
      };
      return texts[priority] || priority;
    },

    // FAQ Methods
    toggleFaqItem(itemId) {
      const index = this.openFaqItems.indexOf(itemId);
      if (index > -1) {
        this.openFaqItems.splice(index, 1);
      } else {
        this.openFaqItems.push(itemId);
      }
    },

    async loadMoreFaq() {
      this.loadingFaq = true;

      // Simulate loading more FAQ
      await new Promise(resolve => setTimeout(resolve, 1500));

      // Add more categories (in real app, load from API)
      this.faqData.push({
        id: 4,
        title: 'حساب کاربری و پروفایل',
        items: [
          {
            id: 401,
            question: 'چگونه اطلاعات پروفایل خود را ویرایش کنم؟',
            answer: 'برای ویرایش اطلاعات پروفایل، به بخش "پروفایل من" در منوی کاربری مراجعه کرده و روی دکمه "ویرایش اطلاعات" کلیک کنید.',
            relatedLinks: [
              { text: 'ویرایش پروفایل', url: '#' }
            ]
          }
        ]
      });

      this.loadingFaq = false;
      this.$bvToast.toast('سؤالات بیشتر بارگذاری شد', {
        title: 'بارگذاری موفق',
        variant: 'success',
        solid: true
      });
    },

    // Live Chat Methods
    async startLiveChat() {
      this.showLiveChat = true;
      this.chatActive = true;
      if (this.isSupportAgent) {
        await this.loadLiveChatSessions();
        this.startLiveChatPolling();
        return;
      }

      // Auto reply after 2 seconds
      try {
        const response = await this.startLiveChatSession({ subject: 'درخواست چت آنلاین' });
        if (!response || !response.status) throw new Error(response?.message || 'خطا در شروع گفتگوی آنلاین');
        this.currentChatSession = response.data;
        this.onlineAgents = response.onlineAgents || this.onlineAgents;
        await this.loadLiveChatMessages(false);
        this.startLiveChatPolling();
      } catch (error) {
        this.$bvToast.toast(error.message || 'خطا در شروع گفتگوی آنلاین', {
          title: 'چت آنلاین',
          variant: 'danger',
          solid: true
        });
      }
    },

    async loadLiveChatSessions() {
      this.loadingChatSessions = true;
      try {
        const response = await this.getLiveChatSessions({ limit: 50 });
        if (response && response.status) {
          this.liveChatSessions = response.data || [];
          this.onlineAgents = response.onlineAgents || this.onlineAgents;
          if (this.currentChatSession) {
            const freshSession = this.liveChatSessions.find(item => item.id === this.currentChatSession.id);
            if (freshSession) this.currentChatSession = freshSession;
          }
        }
      } finally {
        this.loadingChatSessions = false;
      }
    },
    async selectLiveChatSession(session) {
      this.currentChatSession = session;
      this.chatActive = session.status !== 'closed';
      this.chatMessages = [];
      await this.loadLiveChatMessages(false);
    },
    async loadLiveChatMessages(onlyNew = true) {
      if (!this.currentChatSession?.id) return;
      const lastId = onlyNew && this.chatMessages.length ? this.chatMessages[this.chatMessages.length - 1].id : 0;
      const response = await this.getLiveChatMessages({
        sessionId: this.currentChatSession.id,
        afterId: lastId
      });
      if (response && response.status) {
        const messages = (response.data || []).map(this.normalizeLiveChatMessage);
        this.currentChatSession = response.session || this.currentChatSession;
        this.chatMessages = onlyNew ? this.chatMessages.concat(messages) : messages;
        this.scrollToChatBottom();
      }
    },

    normalizeLiveChatMessage(message) {
      const currentNationalId = this.currentUser?.national_id || this.currentUser?.nationalId || this.currentUser?.id;
      const mine = message.senderNationalId && String(message.senderNationalId) === String(currentNationalId);
      const isSystem = message.senderType === 'system';
      return {
        id: message.id,
        sender: isSystem ? 'support' : mine ? 'user' : 'support',
        senderName: isSystem ? 'سامانه' : message.senderName,
        text: message.text,
        time: this.formatChatTime(message.createdAt)
      };
    },

    async sendChatMessage() {
      if (!this.chatInput.trim() || !this.currentChatSession?.id) return;
      const userMessage = this.chatInput.trim();
      this.chatInput = '';

      try {
        const response = await this.sendLiveChatMessage({
          sessionId: this.currentChatSession.id,
          message: userMessage
        });
        if (!response || !response.status) throw new Error(response?.message || 'خطا در ارسال پیام');
        this.chatMessages.push(this.normalizeLiveChatMessage(response.data));
        await this.loadLiveChatMessages(true);
        if (this.isSupportAgent) await this.loadLiveChatSessions();
      } catch (error) {
        this.chatInput = userMessage;
        this.$bvToast.toast(error.message || 'خطا در ارسال پیام', {
          title: 'چت آنلاین',
          variant: 'danger',
          solid: true
        });
      }
    },

    sendQuickResponse(text) {
      this.chatInput = text;
      this.sendChatMessage();
    },

    attachFileToChat() {
      // In real app, implement file attachment
      this.$bvToast.toast('ارسال فایل در گفتگوی آنلاین فعال نیست؛ برای فایل، تیکت ثبت کنید.', {
        title: 'اطلاع',
        variant: 'info',
        solid: true
      });
    },

    downloadChat() {
      const chatContent = this.chatMessages.map(msg =>
        `${msg.senderName || (msg.sender === 'user' ? 'شما' : 'پشتیبان')} (${msg.time}): ${msg.text}`
      ).join('\n\n');

      const blob = new Blob([chatContent], { type: 'text/plain;charset=utf-8' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `live_chat_${this.currentChatSession?.id || new Date().getTime()}.txt`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    },

    async endChat() {
      if (this.currentChatSession?.id && this.currentChatSession.status !== 'closed') {
        await this.closeLiveChatSession({ sessionId: this.currentChatSession.id });
      }
      this.stopLiveChatPolling();
      this.showLiveChat = false;
      this.chatMessages = [];
      this.currentChatSession = null;
      this.chatInput = '';
      this.chatActive = false;
    },
    handleLiveChatModalHide() {
      this.stopLiveChatPolling();
      this.chatMessages = [];
      this.currentChatSession = null;
      this.chatInput = '';
      this.chatActive = false;
    },
    startLiveChatPolling() {
      this.stopLiveChatPolling();
      this.liveChatPollingId = setInterval(async () => {
        if (!this.showLiveChat) return;
        if (this.isSupportAgent) await this.loadLiveChatSessions();
        await this.loadLiveChatMessages(true);
      }, 5000);
    },

    stopLiveChatPolling() {
      if (this.liveChatPollingId) {
        clearInterval(this.liveChatPollingId);
        this.liveChatPollingId = null;
      }
    },

    getChatStatusText(status) {
      const statuses = {
        waiting: 'در انتظار پاسخ',
        active: 'آنلاین',
        closed: 'بسته شده'
      };
      return statuses[status] || 'آماده';
    },

    formatChatTime(value) {
      if (!value) return this.getCurrentTime();
      const date = new Date(String(value).replace(' ', 'T'));
      if (Number.isNaN(date.getTime())) return this.getCurrentTime();
      return date.toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
    },
    scrollToChatBottom() {
      this.$nextTick(() => {
        const container = this.$refs.chatMessages;
        if (container) {
          container.scrollTop = container.scrollHeight;
        }
      });
    },

    // Ticket Reply Methods
    async submitTicketReply() {

      if (!this.ticketReply.trim() || !this.selectedTicket?.canReply) return;
      try {
        const response = await this.replySupportTicket({
          ticketId: this.selectedTicket.id,
          message: this.ticketReply,
          closeTicket: this.closeTicketAfterReply
        });
        if (!response || !response.status) {
          throw new Error(response?.message || 'خطا در ارسال پاسخ');
        }
        this.ticketReply = '';
        this.closeTicketAfterReply = false;
        await this.loadSupportTickets();
        const refreshedTicket = this.tickets.find(ticket => String(ticket.id) === String(this.selectedTicket.id));
        if (refreshedTicket) this.selectedTicket = refreshedTicket;
        this.$bvToast.toast('پاسخ شما ارسال شد', {
          title: 'ارسال موفق',
          variant: 'success',
          solid: true
        });
      } catch (error) {
        console.error('Ticket reply error:', error);
        this.$bvToast.toast(error.message || 'خطا در ارسال پاسخ', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      }
    },

    async closeTicket() {
      if (!this.selectedTicket?.canReply) return;
      try {
        const response = await this.replySupportTicket({
          ticketId: this.selectedTicket.id,
          message: this.ticketReply,
          closeTicket: true
        });

        if (!response || !response.status) {
          throw new Error(response?.message || 'خطا در بستن تیکت');
        }

        this.ticketReply = '';
        await this.loadSupportTickets();
        const refreshedTicket = this.tickets.find(ticket => String(ticket.id) === String(this.selectedTicket.id));
        if (refreshedTicket) this.selectedTicket = refreshedTicket;
        this.$bvToast.toast('تیکت با موفقیت بسته شد', {
          title: 'بستن تیکت',
          variant: 'success',
          solid: true
        });
      } catch (error) {
        console.error('Close ticket error:', error);
        this.$bvToast.toast(error.message || 'خطا در بستن تیکت', {
          title: 'خطا',
          variant: 'danger',
          solid: true
        });
      }
    },

    // Contact Methods
    openSocial(platform) {
      const urls = {
        telegram: 'https://t.me/farhangian_support',
        instagram: 'https://instagram.com/farhangian_election',
        twitter: 'https://twitter.com/farhangian_vote',
        whatsapp: 'https://wa.me/989123456789'
      };

      if (urls[platform]) {
        window.open(urls[platform], '_blank');
      }
    },

    // Utility Methods
    getCurrentDate() {
      return new Date().toLocaleDateString('fa-IR');
    },

    getCurrentTime() {
      return new Date().toLocaleTimeString('fa-IR', { hour: '2-digit', minute: '2-digit' });
    }
  }
};
</script>

<style scoped>
.support-page {
  background: linear-gradient(135deg, #f8f9fa 0%, #e8ecef 100%);
  min-height: 100vh;
  padding-bottom: 50px;
}

/* Header */
.support-header {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.support-icon {
  font-size: 2.5rem;
  color: #2196F3;
}

.support-status {
  background: rgba(255, 255, 255, 0.1);
  padding: 15px;
  border-radius: 10px;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.response-time {
  font-size: 0.9rem;
  opacity: 0.9;
}

/* Quick Actions */
.action-card {
  border-radius: 12px;
  border: none;
  box-shadow: 0 5px 15px rgba(0, 0, 0, 0.08);
  transition: all 0.3s ease;
  cursor: pointer;
  height: 100%;
}

.action-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.15);
}

.action-icon {
  width: 70px;
  height: 70px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto;
  font-size: 2rem;
}

.tickets {
  background: linear-gradient(135deg, #FF9800 0%, #FFC107 100%);
  color: white;
}

.new {
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
  color: white;
}

.faq {
  background: linear-gradient(135deg, #4CAF50 0%, #8BC34A 100%);
  color: white;
}

.chat {
  background: linear-gradient(135deg, #9C27B0 0%, #E91E63 100%);
  color: white;
}

/* Tickets */
.ticket-item {
  background: white;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
  padding: 15px;
  margin-bottom: 15px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.ticket-item:hover {
  background: #f8f9fa;
  border-color: #2196F3;
}

.ticket-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.ticket-title {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
}

.ticket-meta {
  display: flex;
  align-items: center;
  font-size: 0.9rem;
  color: #666;
}

.ticket-id {
  background: #f0f0f0;
  padding: 2px 8px;
  border-radius: 4px;
  margin-right: 10px;
  font-family: monospace;
}

.ticket-date {
  margin-right: 10px;
}

.ticket-body {
  color: #555;
}

.ticket-preview {
  margin-bottom: 10px;
  line-height: 1.6;
}

.ticket-info {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: #666;
  border-top: 1px solid #f0f0f0;
  padding-top: 10px;
}

.category,
.last-update {
  display: flex;
  align-items: center;
}

/* FAQ */
.faq-item {
  border-bottom: 1px solid #f0f0f0;
  padding: 15px 0;
  cursor: pointer;
}

.faq-item:last-child {
  border-bottom: none;
}

.faq-question {
  font-weight: 600;
  color: #2c3e50;
  display: flex;
  align-items: center;
  transition: color 0.2s ease;
}

.faq-item:hover .faq-question {
  color: #2196F3;
}

.faq-answer {
  padding: 15px 0 0 25px;
  color: #555;
  line-height: 1.8;
}

.related-links {
  margin-top: 15px;
  padding: 10px;
  background: #f8f9fa;
  border-radius: 6px;
  border-right: 3px solid #2196F3;
}

.related-links ul {
  padding-right: 20px;
  margin-top: 5px;
}

.related-links li {
  margin-bottom: 5px;
}

.related-links a {
  color: #2196F3;
  text-decoration: none;
}

.related-links a:hover {
  text-decoration: underline;
}

/* New Ticket Form */
.file-preview {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8f9fa;
  border-radius: 6px;
  margin-bottom: 8px;
  border: 1px solid #e0e0e0;
}

.file-info {
  display: flex;
  align-items: center;
  flex: 1;
}

.file-name {
  margin: 0 10px;
  flex: 1;
  font-size: 0.9rem;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-size {
  color: #666;
  font-size: 0.8rem;
}

/* Contact Information */
.contact-card {
  border-radius: 12px;
  border: 2px solid #e3f2fd;
  background: linear-gradient(135deg, #f8f9fa 0%, #e8ecef 100%);
}

.contact-methods {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.contact-item {
  display: flex;
  align-items: center;
  padding: 15px;
  background: white;
  border-radius: 10px;
  border: 1px solid #e0e0e0;
  transition: all 0.2s ease;
}

.contact-item:hover {
  border-color: #2196F3;
  box-shadow: 0 3px 10px rgba(33, 150, 243, 0.1);
}

.contact-icon {
  width: 50px;
  height: 50px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  margin-left: 15px;
  flex-shrink: 0;
}

.contact-item.phone .contact-icon {
  background: linear-gradient(135deg, #4CAF50 0%, #8BC34A 100%);
  color: white;
}

.contact-item.email .contact-icon {
  background: linear-gradient(135deg, #2196F3 0%, #03A9F4 100%);
  color: white;
}

.contact-item.address .contact-icon {
  background: linear-gradient(135deg, #FF9800 0%, #FFC107 100%);
  color: white;
}

.contact-details {
  flex: 1;
}

.contact-title {
  font-weight: 600;
  color: #2c3e50;
  margin-bottom: 5px;
}

.contact-value {
  color: #333;
  margin-bottom: 5px;
}

.contact-hours small {
  font-size: 0.8rem;
}

.social-icons {
  display: flex;
  justify-content: center;
  gap: 10px;
}

.social-btn {
  width: 45px;
  height: 45px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
}

/* Live Chat */
.live-chat-container {
  height: 500px;
  display: flex;
  flex-direction: column;
}

.chat-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: #f8f9fa;
  border-bottom: 1px solid #e0e0e0;
  border-radius: 8px 8px 0 0;
}

.agent-info {
  display: flex;
  align-items: center;
}

.agent-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: #2196F3;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  margin-left: 10px;
}

.agent-name {
  display: flex;
  align-items: center;
  margin-bottom: 3px;
}

.agent-status {
  font-size: 0.85rem;
  color: #666;
}

.chat-actions {
  display: flex;
  gap: 5px;
}

.chat-session-list {
  max-height: 150px;
  overflow-y: auto;
  border: 1px solid #e3e7ef;
  border-radius: 10px;
  padding: 10px;
  background: #fbfcff;
}

.chat-session-item {
  padding: 10px 12px;
  border: 1px solid #e0e6f2;
  border-radius: 8px;
  background: #fff;
  cursor: pointer;
  margin-bottom: 8px;
  transition: all 0.2s ease;
}

.chat-session-item:hover,
.chat-session-item.active {
  border-color: #2196F3;
  box-shadow: 0 4px 12px rgba(33, 150, 243, 0.12);
  transform: translateY(-1px);
}

.message-author {
  font-size: 0.75rem;
  font-weight: 700;
  color: #4b5d73;
  margin-bottom: 4px;
}

.chat-messages {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
  background: #f5f5f5;
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.message {
  max-width: 80%;
}

.message.user {
  align-self: flex-start;
}

.message.support {
  align-self: flex-end;
}

.message-content {
  padding: 12px 15px;
  border-radius: 15px;
  position: relative;
}

.message.user .message-content {
  background: white;
  border: 1px solid #e0e0e0;
  border-bottom-right-radius: 5px;
}

.message.support .message-content {
  background: #e3f2fd;
  border: 1px solid #bbdefb;
  border-bottom-left-radius: 5px;
}

.message-time {
  font-size: 0.75rem;
  color: #666;
  margin-top: 5px;
  text-align: left;
}

.chat-input {
  padding: 15px;
  background: white;
  border-top: 1px solid #e0e0e0;
  border-radius: 0 0 8px 8px;
}

.chat-options {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

/* Ticket Details */
.ticket-detail-header {
  padding-bottom: 15px;
  border-bottom: 1px solid #e0e0e0;
}

.ticket-conversation {
  max-height: 400px;
  overflow-y: auto;
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
}

.conversation-message {
  margin-bottom: 20px;
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.sender-info {
  display: flex;
  align-items: center;
  color: #2c3e50;
}

.message-body {
  background: white;
  padding: 15px;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

.message-user .message-body {
  border-right: 3px solid #2196F3;
}

.message-support .message-body {
  border-right: 3px solid #4CAF50;
}

.message-attachments {
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px dashed #e0e0e0;
}

.attachment-item {
  display: flex;
  align-items: center;
  margin-bottom: 5px;
}

.attachment-item a {
  color: #2196F3;
  text-decoration: none;
}

.attachment-item a:hover {
  text-decoration: underline;
}

.ticket-reply {
  padding: 20px;
  background: #f8f9fa;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
}

/* Support Stats */
.support-stats {
  background: linear-gradient(135deg, #2c3e50 0%, #4a6491 100%);
  color: white;
  margin-top: 40px;
}

.stat-item {
  padding: 20px;
}

.stat-number {
  font-size: 2.5rem;
  font-weight: bold;
  margin-bottom: 5px;
}

.stat-label {
  font-size: 0.9rem;
  opacity: 0.9;
}

/* Responsive Design */
@media (max-width: 768px) {
  .support-status {
    margin-top: 15px;
    text-align: center;
  }

  .ticket-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .ticket-meta {
    margin-top: 10px;
    flex-direction: column;
    align-items: flex-start;
    gap: 5px;
  }

  .ticket-info {
    flex-direction: column;
    gap: 10px;
  }

  .contact-item {
    flex-direction: column;
    text-align: center;
  }

  .contact-icon {
    margin-left: 0;
    margin-bottom: 15px;
  }

  .social-icons {
    flex-wrap: wrap;
  }

  .live-chat-container {
    height: 400px;
  }

  .message {
    max-width: 90%;
  }
}
</style>