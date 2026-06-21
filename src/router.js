import Vue from "vue";
import VueRouter from "vue-router";
import AuthGuard from "./utils/auth.guard";
import { UserRole, RequestStatus } from '@/utils/auth.roles'

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "landing",
    component: () => import("./views/Landing"),
  },
  {
    path: "/sso",
    component: () => import(/* webpackChunkName: "sso" */ "./views/home/callbacksso"),
  },{
    name: "home",
    path: '/home',
    component: () => import(/* webpackChunkName: "home" */ "./views/home/home"),
    meta: { loginRequired: true },
  }, {
    name: "Faq",
    path: '/Faq',
    component: () => import(/* webpackChunkName: "Faq" */ "./views/home/Faq"),
    meta: { loginRequired: true },
  }, {
    name: "Contact",
    path: '/Contact',
    component: () => import(/* webpackChunkName: "Contact" */ "./views/home/contact"),
    meta: { loginRequired: true },
  }, {
    name: "Notifications",
    path: '/Notifications',
    component: () => import(/* webpackChunkName: "Notifications" */ "./views/home/Notifications"),
    meta: { loginRequired: true },
  }, {
    name: "Profile",
    path: '/profile',
    component: () => import(/* webpackChunkName: "profile" */ "./views/home/Profile"),
    meta: { loginRequired: true },
  }, {
    path: "/supervisor",
    component: () =>
      import(/* webpackChunkName: "supervisor" */ "./views/home"),
    children: [
      {
        name: "supervisor-dashboard",
        path: 'supervisor-dashboard',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/supervisor-dashboard"),
        meta: {
          loginRequired: true,
          roles: [UserRole.SUPERVISOR]
        }
      },{
        name: "executive-dashboard",
        path: 'executive-dashboard',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/executive-dashboard"),
        meta: {
          loginRequired: true,
          roles: [UserRole.EXECUTIVE]
        }
      },{
        name: "system-schedule",
        path: 'system-schedule',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/system-schedule"),
        meta: {
          loginRequired: true,
          // roles: [UserRole.SUPERVISOR]
        }
      },{
        name: "UsersManagment",
        path: 'UsersManagment',
        component: () => import(/* webpackChunkName: "UsersManagment" */ "./views/home/UsersManagment"),
        meta: {
          loginRequired: true,
         roles: [UserRole.ADMIN, UserRole.SUPERVISOR],
        }
      },{
        name: "user-vote-search",
        path: 'user-vote-search',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/user-vote-search"),
        meta: {
          loginRequired: true,
          roles: [UserRole.ADMIN, UserRole.SUPERVISOR]
        }
      },{
        name: "logs",
        path: 'logs',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/logs"),
        meta: {
          loginRequired: true,
          roles: [UserRole.ADMIN]
        }
      },{
        name: "announcements",
        path: 'announcements',
        component: () => import(/* webpackChunkName: "supervisor" */ "./views/home/Announcements"),
        meta: {
          loginRequired: true,
          roles: [UserRole.ADMIN, UserRole.SUPERVISOR, UserRole.EXECUTIVE]
        }
      }
    ]
  },
  {
    path: "/results",
    component: () =>
      import(/* webpackChunkName: "results" */ "./views/home"),
    children: [
      {
        name: "live-election",
        path: 'live-election',
        component: () => import(/* webpackChunkName: "results" */ "./views/home/live-election"),
        meta: { loginRequired: true,
          roles: [UserRole.ADMIN] },
      }, {
        name: "final-election",
        path: 'final-election',
        component: () => import(/* webpackChunkName: "results" */ "./views/home/final-election"),
        meta: { loginRequired: true },
      },
    ]
  },
  {
    path: "/User",
    component: () =>
      import(/* webpackChunkName: "User" */ "./views/home"),
    children: [
      {
        name: "ViewAdvertise",
        path: 'ViewAdvertise',
        component: () => import(/* webpackChunkName: "User" */ "./views/home/ViewAdvertise"),
        meta: { loginRequired: true },
      }, {
        name: "votingPage",
        path: 'votingPage',
        component: () => import(/* webpackChunkName: "User" */ "./views/home/votingPage"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER,UserRole.CANDIDATE,UserRole.EXECUTIVE,UserRole.SUPERVISOR],
        }
      },
    ]
  },
  {
    path: "/candidate",
    component: () =>
      import(/* webpackChunkName: "candidate" */ "./views/home"),
    // redirect: `home`,
    children: [
      {
        name: "request",
        path: 'request',
        component: () => import("./views/home/request"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER],
          allowedStatuses: [RequestStatus.DRAFT]
        }
      },{
        name: "objection",
        path: 'objection',
        component: () => import("./views/home/objection"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER,UserRole.CANDIDATE],
          allowedStatuses: [RequestStatus.SUPERVISION_REJECTED]
        }
      },
      {
        name: "AcceptConditions",
        path: 'AcceptConditions',
        component: () => import("./views/home/AcceptConditions"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER, UserRole.CANDIDATE],
          allowedStatuses: [RequestStatus.CANDIDATE]
        }
      },
      {
        name: "UploadDocuments",
        path: 'UploadDocuments',
        component: () => import("./views/home/UploadDocuments"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER, UserRole.CANDIDATE],
          allowedStatuses: [RequestStatus.CONDITIONS_ACCEPTED]
        }
      },
      {
        name: "Confirmation",
        path: 'Confirmation',
        component: () => import("./views/home/Confirmation"),
        meta: {
          loginRequired: true,
          roles: [UserRole.VOTER, UserRole.CANDIDATE],
          allowedStatuses: [RequestStatus.DOCUMENTS_UPLOADED]
        }
      },
      {
        name: "Advertise",
        path: 'Advertise',
        component: () => import(/* webpackChunkName: "candidate" */ "./views/home/Advertise"),
        meta: {
          loginRequired: true,
          roles: [UserRole.CANDIDATE],
          allowedStatuses: [RequestStatus.SUPERVISION_APPROVED]
        },
      },
    ]
  },
  {
    path: "/error",
    component: () => import(/* webpackChunkName: "home" */ "./views/Error")
  },
  {
    path: "/unauthorized",
    component: () => import(/* webpackChunkName: "home" */ "./views/Unauthorized")
  },
  {
    path: "*",
    component: () => import(/* webpackChunkName: "home" */ "./views/Error")
  }
];

const router = new VueRouter({
  linkActiveClass: "active",
  routes,
  mode: "history",
});
router.beforeEach(AuthGuard);
export default router;
