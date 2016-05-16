import moment from 'moment';
import "moment/locale/pl"

export class DateFromNowValueConverter {
    toView(value) {
        moment.locale('pl');
        return moment(value).fromNow();
    }
}
