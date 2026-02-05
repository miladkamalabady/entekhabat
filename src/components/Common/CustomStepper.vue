<template>
  <div class="custom-stepper">
    <b-row class="text-center stepper-row" no-gutters>
      <template v-for="(step, index) in steps">

      <b-col
    :xs="responsive.xs"
    :xxs="responsive.xxs"
    class="position-relative stepper-col"
  >
        <div 
          v-if="index > 0" 
          class="stepper-connector"
          :class="{ 'connector-completed': currentStep >= index}"
        ></div>

        <div 
          @click="!disabled  && handleStepClick(index)" 
          class="cursor-pointer stepper-item p-3"
          :class="{
            'stepper-item-active': currentStep === index,
            'stepper-item-completed': currentStep > index ,
            'stepper-item-rejected': (index==1 && requestStatus ==='EXECUTIVE_REJECTED') || (index==3 && requestStatus ==='SUPERVISION_REJECTED'),
            'stepper-item-disabled': disabled 
          }"
        >
          <div class="stepper-number mb-2" :class="(index==1 && requestStatus ==='EXECUTIVE_REJECTED') || (index==3 && requestStatus ==='SUPERVISION_REJECTED')?'stepper-number-rejected' :''">
           {{ index + 1 }}
          </div>

          <h6 class="font-700 mb-0 stepper-title">
            {{ step.title || `مرحله ${index + 1}` }} 
          </h6>

          <div 
            v-if="step.description && showDescription" 
            class="stepper-description mt-1"
          >
            {{ step.description }} 
          </div>
        </div>
      </b-col>

    </template>
    </b-row>

    <!-- <div class="stepper-progress-container">
      <div 
        class="stepper-progress-bar"
        :style="{ width: `${progressPercentage}%` }"
      ></div>
    </div> -->
  </div>
</template>


<script>
import { mapGetters, mapMutations } from "vuex";
  export default {
    name: 'CustomStepper',
  
    props: {
      steps: {
        type: Array,
        required: true,
        default: () => [ ]
      },
      currentStep: {
        type: Number,
        default: 0,
        validator: (value) => value >= 0
      },
      disabled: {
        type: Boolean,
        default: false
      },
  
      showDescription: {
        type: Boolean,
        default: false
      },
  
      colors: {
        type: Object,
        default: () => ({
          active: '#007bff',
          completed: '#28a745',
          inactive: '#e9ecef',
          textActive: '#007bff',
          textCompleted: '#28a745',
          textInactive: '#6c757d',
          progress: '#28a745'
        })
      },
  
      responsive: {
        type: Object,
        default: () => ({
          xs: '4',
          xxs: '6'
        })
      }
    },
  
    computed: {
      ...mapGetters(["requestStatus"]),
      progressPercentage() {
        if (this.steps.length <= 1) return 0
        return (this.currentStep / (this.steps.length - 1)) / 100
      }
    },
  
    methods: {
      handleStepClick(index) {
        if (index < 0 || index >= this.steps.length) return
  
        this.$emit('step-change', index)
        this.$emit('step-click', {
          index,
          step: this.steps[index]
        })
      }
    }
  }
  </script>

<style scoped>
  .custom-stepper {
    position: relative;
    padding: 1.5rem 0 0.5rem;
    width: 100%;
  }
  
  .stepper-row {
    position: relative;
    z-index: 2;
  }
  
  .stepper-col {
    position: relative;
  }
  
  .stepper-connector {
    position: absolute;
    top: 30px;
    right: -50%;
    width: 100%;
    height: 2px;
    background-color: #e9ecef;
    z-index: 1;
    pointer-events: none;
  }
  
  .connector-completed {
    background-color: #28a745;
  }
  
  .stepper-item {
    position: relative;
    z-index: 2;
    transition: all 0.3s ease;
    /* min-height: 120px; */
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    background-color: transparent;
  }
  
  .stepper-item:hover:not(.stepper-item-disabled) {
    background-color: rgba(0, 0, 0, 0.02);
    transform: translateY(-2px);
  }
  
  .stepper-item-active {
    /* border-bottom-color: #007bff !important; */
    background-color: rgba(0, 123, 255, 0.05) !important;
  }
  
  .stepper-item-completed {
    border-bottom-color: #28a745 !important;
  }
  
  .stepper-item-disabled {
    cursor: not-allowed;
    opacity: 0.7;
  }

  .stepper-number {
    width: 25px;
    height: 25px;
    border-radius: 50%;
    background-color: #e9ecef;
    color: #6c757d;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.7rem;
    border: 2px solid #dee2e6;
    transition: all 0.3s ease;
    line-height: 10px;
  }
  
  .stepper-item-active .stepper-number {
    background-color: #007bff;
    color: white;
    border-color: #007bff;
    transform: scale(1.1);
    box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
  }
  
  .stepper-item-completed .stepper-number {
    background-color: #28a745;
    color: white;
    border-color: #28a745;
  }
  
  .stepper-title {
    font-size: 0.75rem;
    color: #6c757d;
    transition: all 0.3s ease;
    text-align: center;
    max-width: 100px;
    word-wrap: break-word;
    line-height: 1.3;
  }
  
  .stepper-item-active .stepper-title {
    color: #007bff;
    font-weight: 800;
  }
  
  .stepper-item-completed .stepper-title {
    color: #28a745;
  }.stepper-item-rejected .stepper-title {
    color: red;
  }.stepper-number-rejected  {
    background-color: red !important;
  }
  
  .stepper-description {
    font-size: 0.75rem;
    color: #adb5bd;
    text-align: center;
    max-width: 100px;
    word-wrap: break-word;
    line-height: 1.2;
  }
  
  .stepper-progress-container {
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 3px;
    background-color: #e9ecef;
    z-index: 1;
    border-radius: 2px 2px 0 0;
  }
  
  .stepper-progress-bar {
    height: 100%;
    background-color: #28a745;
    transition: width 0.4s ease;
    border-radius: 2px 2px 0 0;
  }
  
  @media (max-width: 768px) {
    .custom-stepper {
      padding: 1rem 0 0.25rem;
    }
  
    .stepper-item {
      min-height: 100px;
      padding: 1rem !important;
    }
  
    .stepper-number {
      width: 35px;
      height: 35px;
      font-size: 1rem;
    }
  
    .stepper-title {
      font-size: 0.8rem;
      max-width: 80px;
    }
  
    .stepper-description {
      font-size: 0.7rem;
      max-width: 80px;
    }
  
    .stepper-connector {
      top: 27px;
    }
  }
  
  @media (max-width: 576px) {
    .stepper-item {
      min-height: 90px;
      padding: 0.75rem !important;
    }
  
    .stepper-number {
      width: 30px;
      height: 30px;
      font-size: 0.9rem;
      margin-bottom: 0.5rem !important;
    }
  
    .stepper-title {
      font-size: 0.75rem;
      max-width: 70px;
    }
  
    .stepper-description {
      display: none;
    }
  
    .stepper-connector {
      display: none;
    }
  
    .stepper-progress-container {
      display: none;
    }
  }
  </style>