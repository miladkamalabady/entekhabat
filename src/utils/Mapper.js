import moment from "moment-jalaali";
export function mapCurrentUser(apiUser) {
    return {
        // ساختار قدیمی
        id: apiUser.id,
        full_name: apiUser.fullName,
        national_id: apiUser.nationalId,
        personnel_code: apiUser.personnelCode,
        org_position_desc: apiUser.orgPositionDesc,
        user_type_code: apiUser.userTypeCode,
        region_id: apiUser.regionId,
        region_name: apiUser.regionName,

        // ساختار جدید
        fullName: apiUser.fullName,
        nationalId: apiUser.nationalId,
        personnelCode: apiUser.personnelCode,
        orgPositionDesc: apiUser.orgPositionDesc,
        userTypeCode: apiUser.userTypeCode,
        regionId: apiUser.regionId,
        regionName: apiUser.regionName,
        roles: apiUser.roles,
        userType: apiUser.userType
    };
}

export function mapLoginResponse(response) {
    return {
        status: response.succeeded && response.data.status,
        token: response.data.token,
        refreshToken: response.data.refreshToken,
        currentUser: mapCurrentUser(response.data.user),
        messages: response.messages ?? []
    };
}
export function mapSaveSchedule(data) {
    return {
        events: (data.events || []).map((item, index) => ({
            id: item.id,
            eventKey: item.eventKey || item.key,
            eventName: item.eventName || item.name,
            startDateUtc: item.startDate
                ? moment(item.startDate, "jYYYY/jMM/jDD HH:mm")
                    .format("YYYY-MM-DDTHH:mm:ss")
                : null,
            endDateUtc: item.endDate
                ? moment(item.endDate, "jYYYY/jMM/jDD HH:mm")
                    .format("YYYY-MM-DDTHH:mm:ss")
                : null,
            sortOrder: item.sortOrder ?? (index + 1)
        }))
    };
}
export function mapGetSchedule(response) {
    return (response.data || []).map(item => ({
        id: item.id,
        event_key: item.eventKey,
        event_name: item.eventName,

        start_date: item.startDateUtc
            ? moment(item.startDateUtc.substring(0, 19), "YYYY-MM-DDTHH:mm:ss")
    .format("jYYYY/jMM/jDD HH:mm")
            : null,

        end_date: item.endDateUtc
            ? moment(item.endDateUtc.substring(0, 19), "YYYY-MM-DDTHH:mm:ss")
    .format("jYYYY/jMM/jDD HH:mm")
            : null,

        sort_order: item.sortOrder
    }));
}

export function mapScheduleConfig(response) {

    const data = response?.data || {};

    const start = data.votingStartDateUtc
        ? moment(
            data.votingStartDateUtc.replace("+00:00", ""),
            "YYYY-MM-DDTHH:mm:ss"
        )
        : null;

    const end = data.votingEndDateUtc
        ? moment(
            data.votingEndDateUtc.replace("+00:00", ""),
            "YYYY-MM-DDTHH:mm:ss"
        )
        : null;

    return {
        id: 1,
        startDate: start ? start.format("YYYY-MM-DD HH:mm:ss") : null,
        EndDate: end ? end.format("YYYY-MM-DD HH:mm:ss") : null,

        create_date: null,
        active: data.isVotingOpen ? 1 : 0,

        startDates: start ? start.format("dddd jDD jMMMM jYYYY") : "",
        startTime: start ? start.format("HH:mm") : "",

        endDates: end ? end.format("dddd jDD jMMMM jYYYY") : "",
        endTime: end ? end.format("HH:mm") : "",

        // برای استفاده‌های بعدی
        currentTimeUtc: data.currentTimeUtc,
        isVotingOpen: data.isVotingOpen
    };
}

export function mapSearchUsers(response) {

    const source = response?.data?.data
        ? response.data
        : response;
// response.data.user.roles = response.data.user.roles.map(role => role.toUpperCase());

    return {
        status: true,

        data: (source.data || []).map(item => ({
            id: item.id,
            national_id: item.nationalId,
            first_name: item.firstName,
            last_name: item.lastName,
            personnel_code: item.personnelCode ? Number(item.personnelCode) : null,
            region_id: item.regionId,
            regionName: item.regionName,
            roles: (item.roles.map(role => role.toUpperCase()) || []).join(","),
            created_at: null,
            provinceCode: item.province?.code ?? null,
            provinceName: item.province?.name ?? item.regionName,
            education: null,
            yearsOfService: null,
            executivePass: item.hasValidMembership,
            supervisorPass: item.hasRequiredExperience && item.hasVerifiedDegree
        })),

        meta: {
            total: source.totalCount,
            page: source.currentPage,
            limit: source.pageSize,
            pages: source.totalPages
        }
    };
}

export function mapSearchUsersRequest(payload) {

    const filters = [];

    if (payload.national_id) {
        filters.push({
            field: "nationalId",
            operator: "eq",
            value: payload.national_id
        });
    }

    if (payload.first_name) {
        filters.push({
            field: "firstName",
            operator: "contains",
            value: payload.first_name
        });
    }

    if (payload.region_id) {
        filters.push({
            field: "regionId",
            operator: "eq",
            value: payload.region_id
        });
    }

    return {
        keyword: payload.search || "",
        pageNumber: payload.page || 1,
        pageSize: payload.limit || 10,
        isActive: true,
        orderBy: [],
        advancedSearch: null,
        advancedFilter: filters.length
            ? {
                  logic: "and",
                  filters
              }
            : null
    };
}

export function mapMaxVotesRegions(response) {

    const result = {};

    Object.keys(response || {}).forEach(provinceCode => {

        result[provinceCode] = response[provinceCode].map(item => ({
            id: item.id,
            name: item.name,
            maxVotes: item.maxVotes
        }));

    });

    return result;
}