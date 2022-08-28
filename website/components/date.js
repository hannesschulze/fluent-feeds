import moment from 'moment';
import { useEffect, useState } from 'react';

export default function Date({ dateString }) {
  const [isMounted, setMounted] = useState(false);
  const formatted = isMounted ? moment(dateString).format('LLL') : moment.utc(dateString).format();

  useEffect(() => {
    setMounted(true);
    return () => setMounted(false);
  }, []);

  return (
    <time dateTime={dateString}>{formatted}</time>
  );
}
