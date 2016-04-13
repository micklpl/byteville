import moment from 'moment';

export class DateFromNowValueConverter {
    toView(value) {
        return moment(value).fromNow();
    }
}
